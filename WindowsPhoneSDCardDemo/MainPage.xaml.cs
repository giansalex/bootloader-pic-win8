using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WindowsPhoneSDCardDemo.Resources;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Phone.Speech.Synthesis;
using Windows.Phone.Speech.Recognition;
using Windows.Storage.Streams;// stream Reader, writer
using Microsoft.Devices; //Vibrar
using System.Windows.Threading ; // Timer
using System.Threading;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media;
using Windows.System.Display;
using Microsoft.Devices.Sensors; // giroscope
using Windows.Storage.Pickers;

using Windows.Phone.Storage.SharedAccess;
using Windows.Storage;
 

namespace WindowsPhoneSDCardDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
       // Microsoft.Xna.Framework.Input.MouseState ms;
        ExternalStorageFolder folderCCS;
        IEnumerable<ExternalStorageFile> filesCCS;// Nivel superior de la carpeta CCS
        StreamReader hexFile;// Buffer de archivo hex
        int nlineas;
        Byte data; // Para recibir dato
        String Device;
        StreamSocket BTSock;      // Socket usado para comunicar con Modulo
        System.Collections.Generic.IReadOnlyList<PeerInformation> peers; //lista de Dispositivos apareados
        PeerInformation requestingPeer;
        int device;
        bool helplist,helpBlue,Conect;
        SpeechSynthesizer TxtVoz; // Nokia Speak
        SpeechRecognizerUI recoWithUI;// Nokia recog
        VibrateController vibrateCont;
        DispatcherTimer Timer;
        string Read;
        string Compilador;
        bool ModeBoot;
        ProgressIndicator prog;
        //FileOpenPicker openPicker;
        bool Lectura;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            helplist = helpBlue = Conect = false;
            Loaded += MainPage_Loaded;
            device = -1;
            TxtVoz = new SpeechSynthesizer(); // Nueva Memory Space para TxtVoz
            BtnSend.IsEnabled = false;
            vibrateCont = VibrateController.Default;
            Configurar_SpeechText();
            Compilador = "CCS";
            data = 10;
            Device = "PIC16f887";
            PeerFinder.TriggeredConnectionStateChanged += ConexionCambiada;
            PeerFinder.ConnectionRequested += ConnectionRequested;

            SystemTray.SetBackgroundColor(this, Colors.Red);//Rojo  
            SystemTray.SetForegroundColor(this, Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));// White 90%
            SystemTray.SetOpacity(this, 1.0);//100%
            SystemTray.SetIsVisible(this, true);
            prog = new ProgressIndicator();
            prog.IsVisible = true;
            prog.IsIndeterminate = false;
            prog.Text = "Windows Phone 8.1";
            SystemTray.SetProgressIndicator(this, prog);
            ModeBoot = true;
            Visible_Boot();
            /**/
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;// Con puntos suspensivos
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.BackgroundColor = Colors.Red;
            ApplicationBar.ForegroundColor = Colors.White;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;
            //ApplicationBarIconButton BtnToPic=new ApplicationBarIconButton();
            ApplicationBarMenuItem imm1 = new ApplicationBarMenuItem("Conectar a Pic");
            imm1.Click += ConectPic_Click;
            ApplicationBarMenuItem imm2 = new ApplicationBarMenuItem("About");
            imm2.Click += SearchHex_Click;
            ApplicationBar.MenuItems.Add(imm1);
            ApplicationBar.MenuItems.Add(imm2);

            /**/
            Timer = new DispatcherTimer();
            Timer.Tick += Timer_Tick;
            Timer.Interval = TimeSpan.FromMilliseconds(100);
            Lectura = false;

            CameraButtons.ShutterKeyPressed += BtnPress;
        }
        private void BtnPress(object sender,EventArgs e)
        {
            Searchpic();
        }
        private void ConectPic_Click(object sender,EventArgs e)
        {
            Searchpic(); 
        }
        private  void SearchHex_Click(object sender, EventArgs e)
        {
            GoSpeak("Realizado el 20 de junio de 2014.");
            GoSpeak("Hoy es "+DateTime.Now.Date.ToString());
        }
        /*
        private async void HexPic()
        {

            
            try
            {
            openPicker = new FileOpenPicker();
 
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");             
            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();
            //var file = await openPicker.PickSingleFileAsync();
            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message);
            }


        }*/
         /*
         protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
           base.OnNavigatedTo(e);
           MessageBox.Show("yes");
           
            string fileToken = NavigationContext.QueryString["file"];


            var file = await SharedStorageAccessManager.CopySharedFileAsync(ApplicationData.Current.LocalFolder, "File.hex", NameCollisionOption.ReplaceExisting, fileToken);
            var stream = await file.OpenReadAsync();
            IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
            await stream.ReadAsync(buffer, (uint)stream.Size, InputStreamOptions.None);
            DataReader reader = DataReader.FromBuffer(buffer);
            MessageBox.Show( reader.ReadString(buffer.Length));
             
        }*/
        private void ConexionCambiada(
        object sender,
        Windows.Networking.Proximity.TriggeredConnectionStateChangedEventArgs e)
        {
            if (e.State == Windows.Networking.Proximity.TriggeredConnectState.Canceled)
                MessageBox.Show("Dispositivo Desconectado");

            if (e.State == Windows.Networking.Proximity.TriggeredConnectState.PeerFound) 
                MessageBox.Show("Device Encontrado");
            MessageBox.Show("Cambio en Bluetooth");
        }
        private void ConnectionRequested(object sender,
        Windows.Networking.Proximity.ConnectionRequestedEventArgs e)
        {
            requestingPeer = e.PeerInformation;
            MessageBox.Show("Connection solicitada por " + requestingPeer.DisplayName + ". " +
                "Click en conectar");
        }
        private async void Timer_Tick(object sender, EventArgs e)
        {
          //  txtBTStatus.Text=DateTime.Now.Second.ToString();
                await Read_data();
                if (Lectura) txtUsart.Text = data.ToString();

        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //await VoiceCommandService.InstallCommandSetsFromFileAsync(
            //new Uri("ms-appx:VoiceCommandDefinition1.xml")); 
           
            if (!PeerFinder.AllowBluetooth)
            {
                MessageBox.Show(" Bluetooth No Disponible");
            }
            else
            {
                try
                {
                    PeerFinder.Start(); // inicia busqueda de dispositivos apareados
                    PeerFinder.AlternateIdentities["Bluetooth:Paired"] = ""; // Borra listas previas de apareados
                    peers = await PeerFinder.FindAllPeersAsync();     // Hace una lista de apareados

                    for (int i = 0; i < peers.Count; i++)
                        lstDevices.Items.Add(peers[i].DisplayName);// Busca nuestro modulo
                    
                    PeerFinder.Stop();
                }
                catch (Exception Err)
                {
                    if ((uint)Err.HResult == 0x8007048F) {
                        GoSpeak("Active Blutud");
                        lstDevices.IsEnabled = false; }

                }
            }
        }
        private async void GoSpeak(String Texto)
        {
            await TxtVoz.SpeakTextAsync(Texto);
        }
        private async Task ListSDCardFileContents()
        {
            // List the first /default SD Card whih is on the device. Since we know Windows Phone devices only support one SD card, this should get us the SD card on the phone.
            ExternalStorageDevice sdCard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();
            if (sdCard != null)
            {
                // Get the root folder on the SD card.
                ExternalStorageFolder sdrootFolder = sdCard.RootFolder;// Folder raiz principal
                folderCCS =await sdrootFolder.GetFolderAsync("CCS"); // Folder raiz/CCS
                listHex.Items.Clear() ; 
                if (folderCCS != null)
                {
                    // List all the files on the root folder.
                    filesCCS = await folderCCS.GetFilesAsync();
                    if (filesCCS != null)    {
                        foreach (ExternalStorageFile file in filesCCS)
                            listHex.Items.Add(file.Name);
                    }
                      else
                            {
                                MessageBox.Show("No hay archivos en Raiz");
                                listHex.IsEnabled = false;
                            }
                        
                 }               
                else
                {
                    MessageBox.Show("Fallo al buscar folder CCS");
                }
            }
            else
            {
                MessageBox.Show("SD Card no encontrada");
            }
            

        }

        private async void buttonOpenSDCard_Click(object sender, RoutedEventArgs e)
        {
            await ListSDCardFileContents();          
            
        }
        private async void listHex_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
         
            if (listHex.SelectedItems != null && helplist)
            {
                nlineas = 0;
                var archOpen = listHex.SelectedItems[0].ToString();
                foreach (ExternalStorageFile file in filesCCS)
                    if (archOpen == file.Name)
                    {
                        Stream s1 = await file.OpenForReadAsync();
                        if (s1 != null || s1.Length == 0)
                        {
                                                   
                            //long streamLength = s.Length;
                            StreamReader sr = new StreamReader(s1);                         
                            while (sr.ReadLine()!=null)
                                nlineas++;
                            vibrateCont.Start(TimeSpan.FromSeconds(1));
                            MessageBox.Show("Lineas = "+Convert.ToString(nlineas));
                            sr.Dispose();                           
                        }
                        s1 = await file.OpenForReadAsync();// vuelve abrir el archivo
                        hexFile = new StreamReader(s1);
                    }
                ShellTile tileAplicacion = ShellTile.ActiveTiles.First();

                StandardTileData tileInfo = new StandardTileData();
                tileInfo.BackTitle = Device;
                tileInfo.BackContent ="Fuente:\r"+ listHex.SelectedItem.ToString() +"\rLineas : "+ Convert.ToString(nlineas)
                    + "\r" +peers[device].DisplayName;
                tileInfo.BackBackgroundImage = new Uri("IMM4.png", UriKind.Relative);
                tileAplicacion.Update(tileInfo);
                /*
                ShellTile tileAplicacion = ShellTile.ActiveTiles.First(); 
                StandardTileData tileInfo = new StandardTileData();
                if (nlineas < 100)              
                tileInfo.Count = nlineas;
                tileInfo.Title =listHex.SelectedItem.ToString();
                tileAplicacion.Update(tileInfo);
                */
                helplist = false;
                
            }
        }
        private void listHex_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            helplist = true;
        }

        private void lstDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstDevices.SelectedItems != null && helpBlue){
          //      device = (int) this.lstDevices.SelectedIndex;      
                string selec = lstDevices.SelectedItems[0].ToString();
                txtBTStatus.Text =selec;

                for(int i=0;i<peers.Count;i++)
                if(peers[i].DisplayName==selec)
                    device=i;

                helpBlue = false;
            }
        }

        private void lstDevices_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            helpBlue = true;
          
        }

        private void Configurar_SpeechText()
        {
            // var spanishRecognizer = InstalledSpeechRecognizers.All.Where(sr => sr.Language == "es-ES").FirstOrDefault();
            this.recoWithUI = new SpeechRecognizerUI();
            recoWithUI.Recognizer.Settings.InitialSilenceTimeout = TimeSpan.FromSeconds(6.0);
            recoWithUI.Recognizer.Settings.BabbleTimeout = TimeSpan.FromSeconds(4.0);
            recoWithUI.Recognizer.Settings.EndSilenceTimeout = TimeSpan.FromSeconds(1.2);
            string[] num = { "buscar pic","Autor","Info","milena" };
            recoWithUI.Recognizer.Grammars.AddGrammarFromList("comand", num);
            // await recoWithUI.Recognizer.PreloadGrammarsAsync();
            recoWithUI.Settings.ListenText = "IMM Corporation";
            recoWithUI.Settings.ExampleText = "-buscar pic \r-Autor \r-Info \r-Ella";
            recoWithUI.Settings.ReadoutEnabled = false; // No repite lo que dije
            recoWithUI.Settings.ShowConfirmation = false;// muestra texto escuchado.
            // recoWithUI.Recognizer.SetRecognizer(spanishRecognizer);
        }
         private async Task VozToText()
        {
            SpeechRecognitionUIResult recoResult = await recoWithUI.RecognizeWithUIAsync();
            Read=recoResult.RecognitionResult.Text;
        }
         
        private async void btnConectar_Click(object sender, RoutedEventArgs e)
        {
            if(device<0){
                MessageBox.Show("Elija dispositivo");
                return;
            }
            prog.IsIndeterminate=true; //Muestra carga...
            if (Conect)
            {
                BTSock.Dispose();
                BTSock = null; 
                btnConectar.Content = "Conectar";
                Conect = false;
                GoSpeak("Conexión Cerrada");         
                prog.IsIndeterminate = false; //Elimina carga
                prog.Text = "Desconectado";
                SystemTray.SetBackgroundColor(this,Color.FromArgb(0xE0,0xFF,0,0));
            }
            else
            {
                BTSock = new StreamSocket(); // Crea a nueva conexion Socket
                bool stateConect = false;
                try
                {
                    await BTSock.ConnectAsync(peers[device].HostName, "1");// Conecta con Modulo. en canal "1"
                }
                catch (Exception err)
                {
                    stateConect = true;
                    GoSpeak("Error al Conectar");
                    switch (SocketError.GetStatus(err.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            MessageBox.Show( "Mac No encontrada");
                            break;
                        case SocketErrorStatus.ConnectionTimedOut:
                            MessageBox.Show("Se Supero el Tiempo de Espera");
                            break;
                    }
                    BTSock.Dispose();
                    BTSock = null;   //Elimina Dir Asignada                
                }
                prog.IsIndeterminate = false; //Elimina carga
                if (stateConect) return; //Finaliza la funcion si no se conecto
                prog.Text = "Conectado";
                SystemTray.SetBackgroundColor(this, Color.FromArgb(0xFF,0xFF,168,0));
                btnConectar.Content = "Desconectar";
                GoSpeak("Conexión Establecida Exitosamente");       
                Conect = true;
                btnConectar.IsEnabled = true;  
            }
            
        }

        private IBuffer ByteToIBuffer(Byte num)
        {
            using (DataWriter pic= new DataWriter())
            {
                pic.WriteByte(num);
                return pic.DetachBuffer();
            }
        }
        private async void Send_byte(byte Dato)
        {
            if (BTSock != null)
            {
                var liberar = await BTSock.OutputStream.FlushAsync();//Vacia datos que no se enviaron
                if (liberar == true)
                {
                    var datab = ByteToIBuffer(Dato); // Convierte byte a IBuffer
                    await BTSock.OutputStream.WriteAsync(datab); //  Envia nuestro mensaje al avr 
                }

            }
            else
                txtBTStatus.Text = "Error de Conexion";
        }
        private  async Task Read_data()
        {
            
            DataReader datos = new DataReader(BTSock.InputStream);
            
            datos.InputStreamOptions = InputStreamOptions.Partial;
            try {
             await datos.LoadAsync(1) ;// Lee un byte
            while (datos.UnconsumedBufferLength > 0) // Lee todos los bytes del buffer de entrada. 
            {
                data = datos.ReadByte();
             //   txtBTStatus.Text+=Convert.ToString(data);
            }
            Lectura = true;
            datos.DetachStream();
            datos.Dispose(); 
            }
            catch(Exception fail)
            {
                Lectura = false;
               //MessageBox.Show( fail.Message);
            }
        }

        private void Send_Line(string data)
        {
            int s, h=12;
            s=h<<2;
            char[] b=new char[data.Length];
            StringReader fr = new StringReader(data);
            fr.Read(b, 0,data.Length);
            if (Device == "18F2550") { 
                foreach (char dat in b)
                   Send_byte((byte)dat);
                  return;
            }
            // Pic16f887
		   if (b[0] == ':')
		   {
			// Envia (Count,(H_addr,L_addr)/2,type hex,Data[count])
		    char[] Conf =new char[4];
            int addr;
            short i;
			addr = (atoi_b16(b[3],b[4]) << 8) + atoi_b16(b[5],b[6]);

			addr /= 2; // Para Pic16f
			
			Conf[0]=atoi_b16((char)b[1], (char)b[2]);   //count 
			Conf[1] = (char)(addr >> 8);			       //H_addr
			Conf[2] = (char)addr;						   //L_addr
			Conf[3] = atoi_b16(b[7],b[8]);//Type hex
			//Envia Configuracion Inicial
			for (i = 0; i <4; i++){
				Send_byte((byte)Conf[i]);
               // Thread.Sleep(1);	 // time de recepcion
			}
			// Envia data
			for ( i = 9; i < b.Length - 2; i += 2){
				Send_byte((byte)atoi_b16(b[i],b[i + 1]));
               // Thread.Sleep(1);	// time de recepcion
			}
		}
		else
			txtBTStatus.Text="Comentario Encontrado" ;
            /*
            foreach(byte dat in b)
            {
                Send_byte(dat);
                //Delay
            }*/
        }
        private  char atoi_b16(char x1,char x2) {  // Convert two hex characters to a int8
			  int result =  0;

				 if (x1 >= 'A')
					 result = 16 * result + x1 - 'A' + 10;
				 else
					 result = 16 * result + x1 - '0';
				 if (x2 >= 'A')
					 result = 16 * result + x2 - 'A' + 10;
				 else
					 result = 16 * result + x2 - '0';

			 return((char)(result));
}
        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if(!ModeBoot&&txtUsart.Text!="")
            {             
                var Instr = Convert.ToInt16(txtUsart.Text);
                if (Instr > 255) {
                    MessageBox.Show("Se supero tamaño de byte");
                return;
                } 
                Send_byte((byte)(Instr&0x00FF));
                txtBTStatus.Text="Dato Enviado:"+txtUsart.Text;
                return;
            }
            prog.IsIndeterminate=true; // Progress Superior
            if(Compilador=="CCS")
            nlineas -= 2; // Por comentarios de CCS
            /**/
            for(int i=0;i<nlineas;i++)
            {
                var strData = hexFile.ReadLine();
                if(i!=nlineas-2) // Evita enviar word de Configuracion;
                {
                if (Compilador == "MikroC" && i == 0) continue; // Evita primera linea
                Send_Line(strData);
                Thread.Sleep(8); // Tiempo de Escritura, por linea (1ms)
                }
                 
            }
            /**/
	Thread.Sleep(2);
	await Read_data(); // Espera check de Pic
	if(data=='K') txtBTStatus.Text = "PIC16F887 OK";
            
            MessageBoxResult resp;
            resp=MessageBox.Show("Desea Iniciar \r Programa", "IMM Bootloader", MessageBoxButton.OKCancel);
             if (resp == MessageBoxResult.OK)
                Send_byte((byte)'i');       
           
            vibrateCont.Start(TimeSpan.FromSeconds(1));
            prog.IsIndeterminate = false;
        }

        private void compCCS_Checked(object sender, RoutedEventArgs e)
        {
            Compilador = "CCS"; txtBTStatus.Text = Compilador;           
        }
 

        private void compMikroc_Checked(object sender, RoutedEventArgs e)
        {
            Compilador = "MikroC"; txtBTStatus.Text = Compilador;
        }

        private async void txtBootLoader_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                await VozToText();// En progreso algun dia
            }
            catch(Exception vr)
            {
                MessageBox.Show(vr.Message);
                return; // finaliza funcion si no hay texto
            }
            switch (Read)
            {
                case "buscar pic": Searchpic();    
                break;
                case "Autor": GoSpeak("Hecho por Yan Carlos Salas");
                    break;
                case "Info": GoSpeak("De I M M Corporation.");
                             GoSpeak("Integrado Por:");
                             GoSpeak("Enrique Sam Muñoz, Venegas Lopez y Yancarlos Salas");
                    break;
                case "milena": GoSpeak("Ella es una chipa");
                    break;
            }
       
            
        }

        private void ModeUSART_Unchecked(object sender, RoutedEventArgs e)
        {
            ModeUSART.Content="Modo :Boot";
            ModeBoot = true;
            Timer.Stop();
            Visible_Boot();

        }

        private void ModeUSART_Checked(object sender, RoutedEventArgs e)
        {
            ModeUSART.Content="Modo :USART";
            ModeBoot=false;
            if(Conect) Timer.Start();
            Visible_Usart();
        }
        private void Visible_Boot()
        {
            label1.Visibility = Visibility.Visible;
            label2.Visibility = Visibility.Visible;
            lstDevices.Visibility = Visibility.Visible;
            listHex.Visibility = Visibility.Visible;
            compCCS.Visibility = Visibility.Visible;
            compMikroc.Visibility = Visibility.Visible;
            txtUsart.Visibility = Visibility.Collapsed;
            label3.Visibility = Visibility.Collapsed;
            btnUp.Visibility = Visibility.Collapsed;
            btnDown.Visibility = Visibility.Collapsed;
        }
        private void Visible_Usart()
        {
        label1.Visibility=Visibility.Collapsed;
        label2.Visibility=Visibility.Collapsed;
        lstDevices.Visibility=Visibility.Collapsed;
        listHex.Visibility=Visibility.Collapsed;
        compCCS.Visibility = Visibility.Collapsed;
        compMikroc.Visibility = Visibility.Collapsed;
        txtUsart.Visibility = Visibility.Visible;
        label3.Visibility = Visibility.Visible;
        btnUp.Visibility = Visibility.Visible;
        btnDown.Visibility = Visibility.Visible;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            var num = Convert.ToInt16(txtUsart.Text);
            num++;
            txtUsart.Text = num.ToString();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            var num = Convert.ToInt16(txtUsart.Text);
            if(num>0) --num;
            txtUsart.Text = num.ToString();
            
        }

        private async void Searchpic()
        {
                txtBTStatus.Text = "Buscando...";
                if (Conect&&ModeBoot)
                    {
                        Send_byte((byte)'i');   //  Envia dato para iniciar Bootloader.
                        Thread.Sleep(20);       // Espera  aque el pic Responda
                        await Read_data();
                        vibrateCont.Start(TimeSpan.FromSeconds(1));
                        if (data == '>') Device = "16F887";
                        if (data == '8') Device = "18F2550";
                        if (data == '>' || data == '8')
                        {
                          //  btnConectar.IsEnabled = true;   
                            MessageBox.Show("Conectado", "PIC"+Device, MessageBoxButton.OK);
                        }
                 
                        
                    }
                txtBTStatus.Text = "Culminado...";

        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("¿Desea Salir?", "IMM", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                e.Cancel = true;     
            else
                e.Cancel = false;
        }
    }
}