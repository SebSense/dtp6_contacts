using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;

namespace dtp6_contacts
{
    class MainClass
    {
        static List<Person> contactList = new();
        class Person
        {
            public string persname, surname, phone, address;
            public List<string> phones = new();
            public List<string> addresses = new();
            public Date birthdate;
            public Person(string persname, string surname, string phone, string address, List<string> phones, List<string> addresses, Date birthdate)
            {
                this.persname = persname;
                this.surname = surname;
                this.phone = phone;
                this.address = address;
                this.phones = phones;
                this.addresses = addresses;
                this.birthdate = birthdate;
            }
            public Person() { }
            public void Print()
            {
                Console.Write($" * {persname} {surname} *\n\tTelephone:");
                if(phones.Count > 0) Console.Write("\t" + phones[0]);
                Console.WriteLine();
                for (int i = 1; i < phones.Count; i++) Console.WriteLine("\t\t\t" + phones[i]);
                Console.Write("\tAddresses:");
                if(addresses.Count > 0) Console.Write("\t" + addresses[0]);
                Console.WriteLine();
                for (int i = 1; i < addresses.Count; i++) Console.WriteLine("\t\t\t" + addresses[i]);
                Console.WriteLine("\tBirthdate:\t" + birthdate.ToString());
            }
        }
        class Date
        {
            public int year = 0;
            public int month = 0;
            public int day = 0;
            public Date(string date)
            {
                string[] dates = date.Split(' ', '-');
                try
                {
                    year = int.Parse(dates[0]);
                    month = int.Parse(dates[1]);
                    day = int.Parse(dates[2]);
                }
                catch(Exception) { Console.WriteLine("Error: Incorrect format when entering birthdate. Using default birthdate. Use 'edit' to correct the contact."); }
                
            }
            public string ToString()
            {
                return $"{year:D4}-{month:D2}-{day:D2}";
            }
        }
        public static void Main(string[] args)
        {
            string lastFileName = "address.lis";
            bool unsavedChanges = false;
            string[] commandLine;
            Console.WriteLine("Hello and welcome to the contact list\nSetting default file... 'address.lis' set as active filename\n");
            ShowCommands();
            do
            {
                commandLine = GetStringArray("> ");
                if (commandLine[0] == "quit")
                {
                    if (!unsavedChanges) { Console.WriteLine("Goodbye!"); return; }
                    else if (QuitSavePrompt(lastFileName)) return;
                    continue;
                }
                if (commandLine[0] == "delete")
                {
                    unsavedChanges = Delete(contactList, commandLine, unsavedChanges);
                }
                else if (commandLine[0] == "edit")
                {
                   unsavedChanges = Edit(contactList, commandLine, unsavedChanges);
                }
                else if (commandLine[0] == "list")
                {
                    List(contactList, commandLine);
                }
                else if (commandLine[0] == "load")
                {
                    unsavedChanges = Load(contactList, commandLine, ref lastFileName, unsavedChanges);
                }
                else if (commandLine[0] == "save")
                {
                    unsavedChanges = Save(contactList, commandLine, ref lastFileName, unsavedChanges);
                }
                else if (commandLine[0] == "new")
                {
                    unsavedChanges = NewContact(contactList, commandLine, unsavedChanges);
                }
                else if (commandLine[0] == "help")
                {
                    ShowCommands();
                }
                else
                {
                    Console.WriteLine($"Unknown command: '{commandLine[0]}'");
                }
            } while (true);
        }

        private static bool Load(List<Person> contactList, string[] commandLine, ref string lastFileName, bool unsavedChanges)
        {
            if (commandLine.Length > 2)
            {
                Console.WriteLine("Error! Too many arguments!");
                return unsavedChanges;
            }
            if (commandLine.Length == 2) lastFileName = commandLine[1];
            using (StreamReader infile = new StreamReader(lastFileName))
            {
                string line;
                if (contactList.Count != 0) unsavedChanges = true;
                while ((line = infile.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    string[] attrs = line.Split('|');
                    Person p = new Person();
                    p.persname = attrs[0];
                    p.surname = attrs[1];
                    foreach (string number in attrs[2].Split(';')) p.phones.Add(number);
                    foreach (string address in attrs[3].Split(';')) p.addresses.Add(address);
                    p.birthdate = new(attrs[4]);
                    contactList.Add(p);
                }
                Console.WriteLine("\n\t" + lastFileName + " successfully loaded to list!");
            }
            return unsavedChanges;
        }
        private static bool NewContact(List<Person> contactList, string[] commandLine, bool unsavedChanges)
        {
            if (commandLine.Length == 1 || commandLine.Length == 3)
            {
                string persname, surname;
                try
                {
                    persname = commandLine[1];
                    surname = commandLine[2];
                    Console.WriteLine("Adding new contact: {0} {1}...", persname, surname);
                }
                catch
                {
                    persname = GetString("personal name: ");
                    surname = GetString("surname: ");
                }
                List<string> phones = new();
                while (true)
                {
                    commandLine[0] = GetString("Add a telephone no ('q' to move on): ");
                    if (commandLine[0] == "q") break;
                    phones.Add(commandLine[0]);
                }
                List<string> addresses = new();
                while (true)
                {
                    commandLine[0] = GetString("Add an address ('q' to move on): ");
                    if (commandLine[0] == "q") break;
                    addresses.Add(commandLine[0]);
                }
                string birthdate = GetString("birthdate (YYYY-MM-DD): ");
                contactList.Add(new Person() { persname = persname, surname = surname, phones = phones, addresses = addresses, birthdate = new(birthdate) });
                return true;
            }
            else Console.WriteLine("Error: Command 'new' only accepts one or three arguments ('new' or 'new /firstname/ /lastname/')");
            return unsavedChanges;
        }

        private static void List(List<Person> contactList, string[] commandLine)
        {
            if (commandLine.Length > 3) return;
            for (int i = 0; i < contactList.Count(); i++)
            {
                if ((commandLine.Length == 1)
                    || (commandLine.Length == 2 && (contactList[i].persname == commandLine[1] || contactList[i].surname == commandLine[1]))
                    || (contactList[i].persname == commandLine[1] && contactList[i].surname == commandLine[2])
                    || (contactList[i].surname == commandLine[1] && contactList[i].persname == commandLine[2]))
                {
                    Console.WriteLine("-------------------------------------");
                    Console.Write($"{i:D3}. ");
                    contactList[i].Print();
                }
            }
        }

        private static bool QuitSavePrompt(string lastFileName)
        {
            Console.WriteLine($"\tUnsaved changes detected. Save changes?\n\ts - save to {lastFileName} and quit\tq - quit without saving\tc - cancel quit command\n>");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case 's':
                        //Save();
                        Console.Write("File saved. ");
                        goto case 'q';
                    case 'q':
                        Console.WriteLine("Goodbye!");
                        return true;
                    case 'c':
                        return false;
                }
            } while (true);
        }

        private static string[] GetStringArray(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine().Split(' ');
        }
        private static string GetString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
        private static void ShowCommands()
        {
            Console.WriteLine("Avaliable commands: " +
                "\n  delete /person/            - delete persons with a certain name" +
                "\n  delete                     - delete all entries in the list" +
                "\n  list                       - list all contacts in the list" +
                "\n  list /person/              - list only those contacts with a certain name" +
                "\n  edit                       " +
                "\n  edit /index/               " +
                "\n  edit /persname/ /surname/  - edit a person" + 
                "\n  load                       - load contact list data from the active file" +
                "\n  list /person/              - list only those contacts with a certain name" +
                "\n  new                        - create new person" +
                "\n  new /persname/ /surname/   - create new person with personal name and surname" +
                "\n  quit                       - quit the program" +
                "\n  save                       - save contact list data to the active file" +
                "\n  save /file/                - set active file and save contact list data" +
                "\n  help                       - show available commands\n");
        }
        private static bool Delete(List<Person> contactList, string[] commandLine, bool unsavedChanges)
            //Deletes all contacts or one contact based on index or all contacts matching 'delete /persname/ /surname/'
            //Returns true if the list has been changed.
        {
            if (commandLine.Length == 1) { contactList.Clear(); return true; }
            if (commandLine.Length == 2)
            { 
                for (int i = 0; i < contactList.Count(); i++)
                    if (contactList[i].persname == commandLine[1] || contactList[i].surname == commandLine[1])
                    {
                        Console.WriteLine(i + ". " + contactList[i].persname + " " + contactList[i].surname + " deleted.");
                        contactList.RemoveAt(i);
                        i--;
                        unsavedChanges = true;
                    }
            }
            else if (commandLine.Length == 3)
            {
                for (int i = 0; i < contactList.Count(); i++)
                {
                    if ((contactList[i].persname == commandLine[1] && contactList[i].surname == commandLine[2]) || (contactList[i].persname == commandLine[2] && contactList[i].surname == commandLine[1]))
                    {
                        Console.WriteLine(i + ". " + contactList[i].persname + " " + contactList[i].surname + " deleted.");
                        contactList.RemoveAt(i);
                        i--;
                        unsavedChanges = true;
                    }
                }
            }
            else Console.WriteLine("Error: too many arguments!");
            return unsavedChanges;
        }
        private static bool Edit(List<Person> contactList, string[] commandLine, bool unsavedChanges)
        //Edits a contact in the list. Command 'edit' shows the list and lets user choose contact by index
        //Command 'edit /index/' directly edits contact at /index.
        //Command 'edit /name/ /name/' edits first contact that match both persname and surname, in any order.
        {
            Person p = null;
            //First if/else-tree catches all three command input variants and either:
            // - finds a match and defines var p to the correct contact
            // - finds no match or syntax error, displays an error message and returns method
            if (commandLine.Length == 1)
            {
                List(contactList, new string[] {"list"});
                commandLine = GetStringArray("\nÄndra vilken (index eller för- och efternamn)? >");
                if (commandLine.Length == 1)
                    try
                    {
                        p = contactList[int.Parse(commandLine[0])];
                    }
                    catch (Exception) { Console.WriteLine("Error: Incorrect index"); return unsavedChanges; }
                else if (commandLine.Length == 2)
                {
                    int i;
                    for (i = 0; i < contactList.Count(); i++)
                    {
                        if ((contactList[i].persname == commandLine[0] && contactList[i].surname == commandLine[1]) || (contactList[i].persname == commandLine[1] && contactList[i].surname == commandLine[0]))
                        {
                            p = contactList[i];
                            break;
                        }
                    }
                    if (p == null)
                    {
                        Console.WriteLine("Hittar ingen sådan person!");
                        return unsavedChanges;
                    }
                }
            }
            else if (commandLine.Length == 2)
            {
                try
                {
                    p = contactList[int.Parse(commandLine[1])];
                }
                catch (Exception) { Console.WriteLine("Error. Couldn't delete person at index: " + commandLine[1]);  return unsavedChanges; }
            }
            else if (commandLine.Length == 3)
            {
                int i;
                for (i = 0; i < contactList.Count(); i++)
                {
                    if ((contactList[i].persname == commandLine[1] && contactList[i].surname == commandLine[2]) || (contactList[i].persname == commandLine[2] && contactList[i].surname == commandLine[1]))
                    {
                        p = contactList[i];
                        break;
                    }
                }
                if (i == contactList.Count()) { Console.WriteLine("Hittar ingen sådan person!"); return unsavedChanges; }
            }
            else
            {
                Console.WriteLine("Error! Too many arguments!");
                return unsavedChanges;
            }
            Console.WriteLine(" What do you want to change?\n\tn /name/ - first name\n\ts /name/ - surname\n\tt add - add telephone number\n\tt del - delete a telephone number\n\tt clr - clear all phone numbers\n" +
                    "\ta add - add an address\n\ta del - delete an address\n\ta clr - clear all addresses\n\td - change birthdate\n\th - display this help\n\tq - quit and go back to main menu");
            while (commandLine[0] != "q")
            {
                commandLine = GetStringArray("Edit " + p.persname + " " + p.surname + " >");
                switch (commandLine[0])
                {
                    case "n":
                        try
                        {
                            p.persname = commandLine[1];
                        }
                        catch (Exception)
                        {
                            p.persname = GetString("Enter new first name: ");
                        }
                        unsavedChanges = true;
                        break;
                    case "s":
                        try
                        {
                            p.surname = commandLine[1];
                        }
                        catch (Exception)
                        {
                            p.surname = GetString("Enter new surname: ");
                        }
                        unsavedChanges = true;
                        break;
                    case "t":
                        try
                        {
                            switch (commandLine[1])
                            {
                                case "add":
                                    p.phones.Add(GetString("Enter new phone no: "));
                                    unsavedChanges = true;
                                    break;
                                case "del":
                                    Console.WriteLine(" {0} {1} phonenumbers:", p.persname, p.surname);
                                    for (int i = 0; i < p.phones.Count; i++)
                                    {
                                        Console.WriteLine("   [{0}]:  {1}", i, p.phones[i]);
                                    }
                                    Console.Write("Delete which number? (index) >");
                                    if (int.TryParse(Console.ReadLine(), out int number) && number >= 0 && number < p.phones.Count())
                                    {
                                        p.phones.RemoveAt(number);
                                        unsavedChanges = true;
                                    }
                                    else Console.WriteLine("Error. Incorrect index");
                                    break;
                                case "clr":
                                    p.phones.Clear();
                                    unsavedChanges = true;
                                    break;
                            }
                        }
                        catch (Exception) { }
                        break;
                    case "a":
                        try
                        {
                            switch (commandLine[1])
                            {
                                case "add":
                                    p.addresses.Add(GetString("Enter new address: "));
                                    unsavedChanges = true;
                                    break;
                                case "del":
                                    //Display addresses with index
                                    Console.WriteLine(" {0} {1} addresses:", p.persname, p.surname);
                                    for (int i = 0; i < p.addresses.Count; i++)
                                    {
                                        Console.WriteLine("   [{0}]:  {1}", i, p.addresses[i]);
                                    }
                                    Console.Write("Delete which address? (index) >");
                                    if (int.TryParse(Console.ReadLine(), out int number) && number >= 0 && number < p.addresses.Count())
                                    {
                                        p.addresses.RemoveAt(number);
                                        unsavedChanges = true;
                                    }
                                    break;
                                case "clr":
                                    p.addresses.Clear();
                                    unsavedChanges = true;
                                    break;
                            }
                        }
                        catch (Exception) { }
                        break;
                    case "d":
                        try
                        {
                            p.birthdate = new(commandLine[1]);
                        }
                        catch (Exception)
                        {
                            Console.Write("GetStringArray new birthdate (YYYY-MM-DD): "); p.birthdate = new(Console.ReadLine());
                        }
                        unsavedChanges = true;
                        break;
                    case "h":
                        Console.WriteLine(" What do you want to change?\n\tn /name/ - first name\n\ts /name/ - surname\n\tt add - add telephone number\n\tt del - delete a telephone number\n\tt clr - clear all phone numbers\n" +
                                        "\ta add - add an address\n\ta del - delete an address\n\ta clr - clear all addresses\n\td - change birthdate\n\th - display this help\n\tq - quit and go back to main menu");
                        break;
                    case "q":
                        break;
                    default:
                        Console.WriteLine("Unrecognized command: " + commandLine[0] + ". Type 'h' for a list of commands.");
                        break;
                }
            }
            return unsavedChanges;
        }
        private static bool Save(List<Person> contactList, string[] commandLine, ref string lastFileName, bool unsavedChanges)
        {
            if (commandLine.Length > 2)
            {
                Console.WriteLine("Error! Too many arguments!");
                return unsavedChanges;
            }
            if (commandLine.Length == 2) lastFileName = commandLine[1];
            using (StreamWriter outfile = new StreamWriter(lastFileName))
            {
                if (contactList.Count > 0)
                {
                    Person p = contactList[0];
                    outfile.Write($"{p.persname}|{p.surname}|{String.Join(";", p.phones)}|{String.Join(";", p.addresses)}|{p.birthdate.ToString()}");
                }
                for (int i = 1; i < contactList.Count; i++)
                {
                    Person p = contactList[i];
                    outfile.Write($"\n{p.persname}|{p.surname}|{String.Join(";", p.phones)}|{String.Join(";", p.addresses)}|{p.birthdate.ToString()}");
                }
                Console.WriteLine("\n\tContact list saved to '" + lastFileName + "'");
            }
            return false;
        }
    }
}
