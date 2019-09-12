using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace WindowsPhoneSDCardDemo
{
    public class FileUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            string tempUri = uri.ToString();

            if (tempUri.Contains("/FileTypeAssociation?"))
            {
                int fileIDIndex = tempUri.IndexOf("file=") + 5;
                string fileID = tempUri.Substring(fileIDIndex);

                string fileUri = string.Format(@"/MainPage.xaml?file={0}", fileID);

                return new Uri(fileUri, UriKind.Relative);
            }
            else
                return uri;
        }
    }
}
