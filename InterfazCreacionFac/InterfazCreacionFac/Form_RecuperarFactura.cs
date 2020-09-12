using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace InterfazCreacionFac
{
    //Inicia la clase.
    public partial class Form_RecuperarFactura : Form
    {
        //Esta variable almacenara la ultima ruta a la que el usuario acceda desde el OpenFileDialog.
        private string rutaActual;
        //Creo el Dataset que leera el XML.
        DataSet ds = new DataSet();
        //Creo un DataTable para guardar los datos que usare en la generacion de la factura.
        DataTable dtTimbreFiscal, dtComprobante, dtEmisor, dtRecetor, dtConcepto, dtTraslado;
        public Form_RecuperarFactura()
        {
            InitializeComponent();
        }

        //Metodo que recrea el codigo QR correspondiente para cada factura. 
        //Las varibles que recibe constituiran el texto que sera mostrado en el codigo QR.
        public void CrearQrFac(string id, string re, string rr, string tt, string fe)
        {
            //Asignamos una Url a una variable que sera agregada al texto del QR.
            string sat = "https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?";
            //Crea e inicializa una variable al tipo QrEncoder.
            var qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
            //Crea otra variable y la inicializa con el resultado de un metodo que codifica la cadena.
            var qrCode = qrEncoder.Encode(sat + id + re + rr + tt + fe);
            //Crea y renderiza la imagen, tambien en esta parte se asigna el nombre de la imagen.
            var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), Brushes.Black, Brushes.White);
            using (var stream = new FileStream("qrcode.png", FileMode.Create))
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
        }

        //Este metodo recibe los campos que formaran parte de la cadena original, este metodo recrea la cadena original.
        public string CrearCadenaOriginalFac(string version, string uuid, string fechaTimbre, string rfcProvCer, string selloDigCFDI, string noCerSAT)
        {
            //Recibe los campos y los concatena. 
            string cadenaOriginal = "||" + version + "|" + uuid + "|" + fechaTimbre + "|" + rfcProvCer + "|" + selloDigCFDI + "|" + noCerSAT + "||";
            //Retorna la cadena ya recreada.
            return cadenaOriginal;
        }

        //Este metodo llena las tablas que seran usadas para llenar el Reporte en la vista previa, tambien recibe la ruta del XML que sera leido.
        public void ObtenerNombreTabla(string path1)
        {
            //Reseteo el dataset para que no haya problemas en su estructura al recibir un nuevo XML.
            ds.Reset();
            //Este metodo recibe la ruta, y el modo de lectura con que sera procesado el XML.
            ds.ReadXml(path1, XmlReadMode.Auto);
            //Una vez cargado el XML se creara un structura relacional de tablas dentro del dataset.
            //Vamos a recorrer cada tabla del dataset para obtener las tablas necesarias e ignorar las tablas que no necesitamos.
            foreach (DataTable tbl in ds.Tables)
            {
                //Si se encuentra la tabla Comprobante la guardamos en la tabla que le corresponde.
                if (tbl.TableName.ToString() == "Comprobante")
                {
                    dtComprobante = tbl;
                }
                //Si se encuentra la tabla Emisor la guardamos en la tabla que le corresponde.
                if (tbl.TableName.ToString() == "Emisor")
                {
                    dtEmisor = tbl;
                }
                //Si se encuentra la tabla Receptor la guardamos en la tabla que le corresponde.
                if (tbl.TableName.ToString() == "Receptor")
                {
                    dtRecetor = tbl;
                }
                //Si se encuentra la tabla Concepto la guardamos en la tabla que le corresponde.
                if (tbl.TableName.ToString() == "Concepto")
                {
                    dtConcepto = tbl;
                }
                //Si se encuentra la tabla Traslado la guardamos en la tabla que le corresponde.
                if (tbl.TableName.ToString() == "Traslado")
                {
                    dtTraslado = tbl;
                }
                //Si se encuentra la tabla TimbreFiscalDigital la guardamos en la tabla que le corresponde.
                if (tbl.TableName.ToString() == "TimbreFiscalDigital")
                {
                    dtTimbreFiscal = tbl;
                }
            }
        }

        //Este metodo generara la factura con base en las tablas ya cargadas anteriormente 
        public void GenerarFacturaXML(string path1)
        {
            //Metodo que recibe el XML, inicializa el dataset y tambien inicializa con valores las tablas 
            ObtenerNombreTabla(path1);
            //Obtiene la colección de filas que pertenecen a la tabla TimbreFiscalDigital
            DataRow row = dtTimbreFiscal.Rows[0];
            //Obtiene la colección de filas que pertenecen a la tabla Emisor
            DataRow row1 = dtEmisor.Rows[0];
            //Obtiene la colección de filas que pertenecen a la tabla Receptor
            DataRow row2 = dtRecetor.Rows[0];
            //Obtiene la colección de filas que pertenecen a la tabla Comprobante
            DataRow row3 = dtComprobante.Rows[0];

            //Obtengo los campos parra crear el QR
            //Obtengo el UUID que viene del XML y le concatenamos valores que haran falta para crear el QR.
            string _uuid = "&id=" + row["UUID"].ToString();

            //Obtengo el RFC del Emisor que viene del XML y le concatenamos valores que haran falta para crear el QR.
            string _rfce = "&re=" + row1["Rfc"].ToString();

            //Obtengo el RFC del Receptor que viene del XML y le concatenamos valores que haran falta para crear el QR.
            string _rfcr = "&rr=" + row2["Rfc"].ToString();

            //Obtengo el total que viene del XML y le concatenamos valores que haran falta para crear el QR.
            string _total = "&tt=" + row3["Total"].ToString();

            //Obtenemos el Sello que viene del XML para crear el QR.
            string _sello = row["SelloCFD"].ToString();

            //Obtenemos unicamente los ultimos 8 caracteres del sello y le concatenamos valores que haran falta para crear el QR.
            _sello = "&fe=" + _sello.Substring(336, 8);

            //Llamamos al metodo para crear la imagen de codigo QR que sera mostrada en la factura y le mandamos los valores anteriormente obtenidos.
            CrearQrFac(_uuid, _rfce, _rfcr, _total, _sello);

            //Obtengo los campos que usaremos para crear la cadena original.
            //Obtengo la version.
            string _version = row["Version"].ToString();

            //Obtengo la uuid.
            string _uuid1 = row["UUID"].ToString();

            //Obtengo la fecha del timbrado.
            string _fechaTim = row["FechaTimbrado"].ToString();

            //Obtengo el rfc del provedor de certificado en este caso el de contpaqi.
            string _rfcProvCer = row["RfcProvCertif"].ToString();

            //Obtengo el sello digital.
            string _selloDigCFDI = row["SelloCFD"].ToString();

            //Obtengo el numero de certificado del sa.
            string _noCerSAT = row["NoCertificadoSAT"].ToString();

            //El reporte ademas de necesitar las tablas para generar la factura, tambien necesita de dos parametros, uno de ellos es la cadena original y el otro es 
            //la imagen QR, esos dos parametros son generados en tiempo de ejecución.
            //Aqui inicializo y agrego al reporte el primer parametro, la cadena original y es necesario mandar el nombre del parametro para que el sistema sepa a que parametr corresponde.
            ReportParameter param = new ReportParameter("cadenaOriginal", CrearCadenaOriginalFac(_version, _uuid1, _fechaTim, _rfcProvCer, _selloDigCFDI, _noCerSAT));

            //Aqui inicializo y agrego al reporte el segundo parametro, la imagen QR.
            ReportParameter param1 = new ReportParameter();

            //Definimos el nombre para el parametro, debe ser el mismo que se definio en el el reporte.
            param1.Name = "imagenQr";

            //Como este parametro es una imagen indicamos la ruta de donde sera tomada la imagen asi como tambien indicamos el nombre.
            param1.Values.Add(@"file:////C:\Users\dsa5\Documents\Visual Studio 2013\Projects\InterfazCreacionFac\InterfazCreacionFac\bin\Debug\qrcode.png");

            //Reiniciamos el reportViewer.
            this.reportViewer1.Reset();
            //Indico el modo de procesamiento en este caso es local.
            this.reportViewer1.ProcessingMode = ProcessingMode.Local;
            //Elimino cualquier origen de datos que pueda estar asociado al reportViewer.
            this.reportViewer1.LocalReport.DataSources.Clear();
            //Indico la ruta donde se encuentra el reporte.
            this.reportViewer1.LocalReport.ReportPath = @"C:\Users\dsa5\Documents\Visual Studio 2013\Projects\InterfazCreacionFac\InterfazCreacionFac\bin\Debug\Reportes\ReportFacInter.rdlc";

            //Aqui inicializo el dataset dsTimbreFiscal y agrego al reporte la tabla que le corresponde.  
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsTimbreFiscal", dtTimbreFiscal.DefaultView));
            //Aqui inicializo el dataset dsComprobante y agrego al reporte la tabla que le corresponde.
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsComprobante", dtComprobante.DefaultView));
            //Aqui inicializo el dataset dsEmisor y agrego al reporte la tabla que le corresponde.
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsEmisor", dtEmisor.DefaultView));
            //Aqui inicializo el dataset dsReceptor y agrego al reporte la tabla que le corresponde.  
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsReceptor", dtRecetor.DefaultView));
            //Aqui inicializo el dataset dsConcepto y agrego al reporte la tabla que le corresponde. 
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsConcepto", dtConcepto.DefaultView));
            //Aqui inicializo el dataset dsImpTraslado y agrego al reporte la tabla que le corresponde. 
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsImpTraslado", dtTraslado.DefaultView));
            //Activo el uso de imagenes externas en verdadero.
            this.reportViewer1.LocalReport.EnableExternalImages = true;
            //Asigno el parametro de la imagen QR.
            this.reportViewer1.LocalReport.SetParameters(param1);
            //Asigno el paramero de la cadena origial.
            this.reportViewer1.LocalReport.SetParameters(param);
            //Actualizo el reporte.
            this.reportViewer1.RefreshReport();

        }

        //Metodo que lee y recupera el esquema del xml y es guardado en la carpeta del programa. 
        public void CrearSchema()
        {
            //Codigo para crear el schema apartir de un xml.
            ds.ReadXml("00CB6074-9B83-4860-A5DE-A8CDAFE7DB17.xml");
            ds.WriteXmlSchema("esquema33FI.xsd");
            MessageBox.Show("Esquema creado ");
        }

        //Boton para volver a la pagina inicial.
        private void volverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Oculta esta pagina..
            this.Hide();
            //Hago un objeto del Form1.
            Form1 ventana = new Form1();
            //Muestro el objeto del Form1.
            ventana.Show();
        }

        //Boton para cerrar la aplicacion.
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //Clic en el boton de actualizar.
        private void actualizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lv1.Items.Clear();
            
            //Creo un nuevo objeto de objeto del tipo openFileDialog.
            OpenFileDialog directorioArchivos = new OpenFileDialog();
            
            //Este objeto abrira un explorador de archivos, aqui indico la ruta inicial del  directorio.             
            directorioArchivos.InitialDirectory = @"C:\Users\dsa5\Desktop\Compartir\Compartir hasta la fecha 28 - Junio - 2018\Migracion_cAT\Xml_Facturas\3_3\Nueva carpeta";
            
            //Indico al filtro que tipo de archivos quiero ver  
            directorioArchivos.Filter = "Xml Files | *.xml";
            
            //Obtiene o establece un valor que indica si el cuadro de diálogo muestra una advertencia cuando el 
            //usuario especifica un nombre de archivo que no existe.
            directorioArchivos.CheckFileExists = true;
           
            //Obtiene o establece un valor que indica si el cuadro de diálogo permite seleccionar varios archivos.
            directorioArchivos.Multiselect = true;
           
            //Obtiene o establece un valor que indica si el cuadro de diálogo restaura el directorio al directorio 
            //seleccionado previamente antes de cerrarse.
            directorioArchivos.RestoreDirectory = true;
           
            //Este evento se da cuando el usuario da clic en aceptar, analiza si se seleccionaron uno o muchos archivos.
            if (directorioArchivos.ShowDialog() == DialogResult.OK)
            {
                //Si los archivos seleccionados son mayores a 1. 
                if (directorioArchivos.SafeFileNames.Count() > 1)
                {
                    //Guardo la ultima ruta.
                    //Creo una variable que guarde los caracteres que busco dentro del nombre y ruta de los arhivos.
                    string b = "\\";
                   
                    //Busco dentro del nombre del archivo la poosicion que me indique la ultima aparicion de la variable b y la guardo en una variable int.
                    int position = directorioArchivos.FileName.LastIndexOf(b);
                    
                    //Aqui inicializo la variable con la ruta del archivo pero elimino el nombre del archivo.
                    rutaActual = @"" + directorioArchivos.FileName.Substring(0, position);

                    //Recorro la lista de archivos.
                    for (int i = 0; i < directorioArchivos.SafeFileNames.Count(); i++)
                    {
                        //Cada que cambia de elemento lo agrega a la lista de la interfaz. 
                        lv1.Items.Add(directorioArchivos.SafeFileNames[i].ToString());
                    }
                }
                //En caso de que solo sea un documento genero la factura directamente.
                else
                {
                    //Le mando el xml  al metodo que generara  la factura.
                    GenerarFacturaXML(directorioArchivos.FileName.ToString());
                }
            }
        }

        //Seleccionar un elemento distinto de la lista.
        private void lv1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Recorro el listView en busca del elemento seleccionado.
            foreach (ListViewItem item in lv1.SelectedItems)
            {
                GenerarFacturaXML(rutaActual + "\\" + item.Text);
            }
        }
    }
}
