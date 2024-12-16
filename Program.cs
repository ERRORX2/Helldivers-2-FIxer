using Microsoft.Win32;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace proj_03;


static class Program
{
   


    [STAThread]
    static void Main()
    {
       
    ApplicationConfiguration.Initialize();
        Application.Run(new HD2_Fixer());

        
    }    
    
}
