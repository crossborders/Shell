using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using System.Data.SqlTypes;
using System.Runtime.Remoting.Messaging;
using System.Net;

namespace Shell
{
    internal class Program
    {

        static void Main(string[] args)
        {
            if (Login())
            {
                Console.WriteLine("Giriş Başarılı! Shell'e Hoşgeldiniz");
                StartShell();
            }
            else
            {
                Console.WriteLine("Giriş Başarısız! Shell Kapatılıyor");

            }
            Console.ReadKey();
        }

        static bool Login()
        {
            REQUERY: 
            Console.WriteLine("Press 1 for Login");
            Console.WriteLine("Press 2 for Sign Up");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.NumPad1)
            {
                Console.Write("Kullanıcı Adı : ");
                String username = Console.ReadLine();

                Console.Write("Şifre : ");
                String password = CryptePassword();

                return ValidateUser(username, password);
            }
            else if (keyInfo.Key == ConsoleKey.NumPad2)
            {
                Console.WriteLine("Lütfen Yöneticinizden Aldığınız Tek Kullanımlık İzin Kodunu Giriniz");
                Console.Write("İzin Kodu : ");
                string permissionCode = Console.ReadLine();
                Console.WriteLine();

                if (Confirmation(permissionCode))
                {
                GITUSERNAME:
                    string username;
                    

                    Console.Write("Kullanıcı Adı : ");
                    username = Console.ReadLine();

                    while (NewUsernameCheck(username) == false)
                    {
                        goto GITUSERNAME;
                    }
                    
                    GITPASSWORD:
                    while(true)
                    {
                        Console.Write("Parola : ");
                        String password = CryptePassword();
                        if (CheckPasswordComplexity(password) == false)
                        {
                            goto GITPASSWORD;
                        }
                        Console.Write("Parola Tekrar : ");
                        String passwordRepeat = CryptePassword();
                        Console.WriteLine() ;
                        if(password == passwordRepeat)
                        {
                            
                            if(SignUp(username, password))
                            {
                               Console.WriteLine("Kullanıcı Başarıyla Eklendi");
                            }
                            else
                            {
                                Console.WriteLine("Kullanıcı Eklenirken Bir Hata Oluştu");
                            }
                            goto REQUERY;
                            
                        }
                        Console.WriteLine("Farklı Tekrarlı Parola Girişi! Yeniden Deneyiniz...");      
                    }

                }

            }
            else
            {
                Console.WriteLine("Geçersiz Kod!");
                return false;
            }
                
            

            Console.WriteLine("Geçersiz Giriş") ;
            return false;

          
        }
            
        static void RemoveUsedPermCodes(string PermissionCode)
        {
            string connectinonString = "Data Source=TEKER\\MSSQLSERVER01;Initial Catalog=myShell;Integrated Security=True";

                using(SqlConnection connection = new SqlConnection(connectinonString))
                {
                    connection.Open();
                    string removeQuery = "DELETE FROM permissionCodes WHERE Codes = @PermissionCode";
                    
                    using(SqlCommand command = new SqlCommand(removeQuery,connection))
                    {
                    command.Parameters.AddWithValue("@PermissionCode", PermissionCode);
                    int rowsAffected = command.ExecuteNonQuery();
                    }
                connection.Close();  
                }
        }

        static bool Confirmation(string code)
        {
            string connectionString = "Data Source=TEKER\\MSSQLSERVER01;Initial Catalog=myShell;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                connection.Open(); 

                string sqlQuery = "SELECT Codes FROM permissionCodes";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string PermissionCode = reader["Codes"].ToString();
                            if (PermissionCode == code)
                            {
                                RemoveUsedPermCodes(PermissionCode);
                                return true;
                            }
                        }

                    }
            
                }
                return false;
        }

        static bool NewUsernameCheck(string username)
        {
            string connectionString = "Data Source=TEKER\\MSSQLSERVER01;Initial Catalog=myShell;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();
            string sqlQuery = "SELECT Username FROM ValidateUser";

            using(SqlCommand command = new SqlCommand (sqlQuery, connection))
            {
                using(SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string usedUserName = reader["Username"].ToString();
                        if(usedUserName == username)
                        {
                            Console.WriteLine("Bu Kullanıcı Adı Zaten Kullanılıyor! Lütfen Yeni Kullanıcı Adı Giriniz...");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        static bool CheckPasswordComplexity(String password)
        {
            if(password == null)
            {
                Console.WriteLine("Parola Boş Olamaz!");
                return false;
            }else if (password.Length < 8)
            {
                Console.WriteLine("Parola 8 Karakterden Kısa Olamaz!");
                return false;
            }
            else
            {
                // Büyük harf, küçük harf ve sembol kontrolü
                bool hasUpper = password.Any(char.IsUpper);
                bool hasLower = password.Any(char.IsLower);
                bool hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

                // Karmaşıklık gereksinimleri
                bool isComplex = hasUpper && hasLower && hasSymbol;
                if(isComplex == false)
                {
                    Console.WriteLine("Parola Karmaşıklık Gereksinimlerini Karşılamıyor!");
                    Console.WriteLine("Parolanız en az 1 Büyük Harf, 1 Özel Karakter İçermelidir...");
                }
                return isComplex;
            }
            
        }

        static bool SignUp(String username , String password)
        {
            string connectionString = "Data Source=TEKER\\MSSQLSERVER01;Initial Catalog=myShell;Integrated Security=True";
            string insertQuery = "INSERT INTO ValidateUser (Username , PasswordHash) VALUES (@username, @passwordHash)";
            
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            using (SHA256 sha256 = SHA256.Create())
            {

                byte[] hashBytes = sha256.ComputeHash(passwordBytes);


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {

                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@passwordHash", hashBytes);

                        connection.Open();
                        var rowsAffected = command.ExecuteNonQuery();
                        connection.Close();
                        return true;

                    }

                }
            }
         }
         

        

        static string CryptePassword()
        {
            string password = "";
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);

                //karakter silme
                if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password.Substring(0, password.Length - 1);
                }
                //girilen karakter geçerliyse ekrana * bastırma
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            }while(keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();

            return password;
        }

        static bool ValidateUser(String username, String password)
        {
            string connectionString = "Data Source=TEKER\\MSSQLSERVER01;Initial Catalog=myShell;Integrated Security=True";
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT Username, PasswordHash FROM ValidateUser WHERE Username = @username";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] PasswordHashBinary = (byte[])reader["PasswordHash"];
                            String storedPasswordHash = BitConverter.ToString(PasswordHashBinary).Replace("-", "").ToLower();
                            
                            
                                //String storedPasswordHash = Encoding.UTF8.GetString(PasswordHashBinary);
                            
                             if(ValidatePassword(password , storedPasswordHash) )
                                {
                                    return true;                            
                                }
                            
                        }
                    }
                }
            }
            
            return false;
        }

        static bool ValidatePassword(String enteredPassword , String storedPasswordHash)
        {
            //
            using (SHA256 sha256 = SHA256.Create())
            {
                
                byte[] enteredPasswordBytes = Encoding.UTF8.GetBytes(enteredPassword);
                byte[] hashBytes = sha256.ComputeHash(enteredPasswordBytes);
                
                
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower() == storedPasswordHash;
                   
            }
        }



        static void StartShell()
        {
            while (true)
            {
                Console.Write("Shell> ");
                string command = Console.ReadLine();

                if (command.ToLower() == "exit")
                {
                    Console.WriteLine("Shell Kapatılıyor...");
                    break;
                }

                //Komut işleme istemi
                ProcessCommand(command);
            }
        }

        static void ProcessCommand(string command) 
        {
            //Komut işleme mantığını yazıyoruz
            Console.WriteLine($"Komut işlendi : {command}");
        }

    }

}
