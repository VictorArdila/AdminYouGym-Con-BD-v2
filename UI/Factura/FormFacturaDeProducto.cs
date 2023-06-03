﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Forms;
using Logica;
using Entidades;
//se importa la libreria para arrastrar formulario
using System.Runtime.InteropServices;
using Font = System.Drawing.Font;

namespace UI
{
    public partial class FormFacturaDeProducto : Form
    {
        Caja cajaRegistradora;
        Cliente cliente;
        CajaRegistradoraService cajaRegistradoraService;
        RutasTxtService rutasTxtService = new RutasTxtService();
        ClienteService clienteService;
        EmpleadoService empleadoService;
        GimnasioService gimnasioService;
        Factura factura;
        Gimnasio gimnasio;
        ProductoService productoService;
        FacturaService facturaService;
        ProductoFacturaTxtService productoTxtService = new ProductoFacturaTxtService();
        ProductoVendidoTxtService productoVendidoTxtService = new ProductoVendidoTxtService();
        List<Factura> facturas;
        List<Gimnasio> gimnasios;
        List<ProductoFacturaTxt> productosFactura = new List<ProductoFacturaTxt>();
        IdUsuarioTxtService idUsuarioTxtService = new IdUsuarioTxtService();
        string fechaDeVenta;
        string rutasVendidos;
        string rutaTxtFacturaVenta;
        public string idEmpleado;
        string nombreDeFactura;
        string notExistFileName;
        int cantidadProductoBD;
        int contadorProductosVendidos;
        //Variables de gimnasio
        string idGimnasio = "#Drog";
        string nombreGimnasio;
        string nitGimnasio;
        string codigoCamara;
        string fraseDistintiva;
        string regimen;
        string pbx;
        string direccion;
        string telefono;

        string referencia;
        //Variables de caja
        double totalFactura = 0;
        string idCajaAbierta;
        double montoActualCaja;
        double montoActualVenta;
        //Variables de productos
        string[] referencias = new string[1000];
        string referenciaProducto;
        int cantidadARestar;
        int cantidadProducto;
        string nombreProducto;
        string detalleProducto;
        double precioProducto;
        double totalProducto;
        string loteProducto;
        string laboratorioProducto;
        string estadoProducto;
        DateTime fechaDeRegistro;
        DateTime fechaDeVencimiento;
        string viaProducto;
        string tipoProducto;
        double precioDeNegocio;
        double porcentajeDeVenta;
        double precioDelProducto;
        //Variables Factura
        int productoLeido = 0;
        string id_factura;
        int secuenciaDeFactura = 0;
        DateTime fechaFactura;
        string nombreEmpleado;
        string ciudad;
        string nombreCliente = "Sin definir";
        double totalSinRedondeo;
        double totalConRedondeo;
        double valorDeRedondeo;
        string formaDePago;
        int i = 0;
        int j = 0;
        public FormFacturaDeProducto()
        {
            cajaRegistradoraService = new CajaRegistradoraService(ConfigConnection.ConnectionString);
            productoService = new ProductoService(ConfigConnection.ConnectionString);
            gimnasioService = new GimnasioService(ConfigConnection.ConnectionString);
            clienteService = new ClienteService(ConfigConnection.ConnectionString);
            empleadoService = new EmpleadoService(ConfigConnection.ConnectionString);
            facturaService = new FacturaService(ConfigConnection.ConnectionString);
            InitializeComponent();
            ObtenerRutaDeVendido();
            CargarArchivo(productoTxtService);
            ConsultarCajaAbierta();
            ConsultarDatosGimnasio();
            SumatoriaDeFactura();
            BuscararGimnasio();
            BuscarInformacionDeEmpleado();
            BuscarSesionDeUsuario();
            CondicionesIniciales();
        }
        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        //***********************************************Metodos*******************************************************
        private void CondicionesIniciales()
        {
            if (cantidadARestar == 0)
            {
                btnSoloVender.Enabled = false;
                btnImprimirFactura.Enabled = false;
            }
        }
        private void ObtenerRutaDeVendido()
        {
            RutasTxtConsultaResponse rutasTxtConsultaResponse = rutasTxtService.Consultar();
            if (rutasTxtConsultaResponse.RutasTxts.Count > 0)
            {
                foreach (var item in rutasTxtConsultaResponse.RutasTxts)
                {
                    rutasVendidos = item.RutaProductosVendidos;
                }
            }
        }
        private void BuscarInformacionDeEmpleado()
        {
            textSearchEmpleado.Text = idEmpleado;
        }
        private void BuscarSesionDeUsuario()
        {
            IdUsuarioTxtConsultaResponse idEmpleadoTxtConsultaResponse = idUsuarioTxtService.Consultar();
            BusquedaEmpleadoRespuesta respuesta = new BusquedaEmpleadoRespuesta();
            string identificacion = "";
            if (idEmpleadoTxtConsultaResponse.Encontrado == true)
            {
                foreach (var item in idEmpleadoTxtConsultaResponse.IdEmpleadoTxts)
                {
                    identificacion = item.Identificacion;
                }
                respuesta = empleadoService.BuscarPorIdentificacion(identificacion);
                if (respuesta.Empleado != null)
                {
                    textNombreEmpleado.Text = respuesta.Empleado.Nombres;
                    textApellidoEmpleado.Text = respuesta.Empleado.Apellidos;
                    textIdentificacionEmpleado.Text = respuesta.Empleado.Identificacion;
                }
            }
            else
            {
                string mensaje = idEmpleadoTxtConsultaResponse.Mensaje;
                MessageBox.Show(mensaje.ToString());
            }
        }
        private void MapearProductosVendidos(string referencia)
        {
            BusquedaProductoRespuesta respuesta = new BusquedaProductoRespuesta();
            respuesta = productoService.BuscarPorReferencia(referencia);
            if (respuesta.Producto != null)
            {
                var productos = new List<Producto> { respuesta.Producto };
                if (contadorProductosVendidos == 1)
                {
                    referenciaProducto = respuesta.Producto.Referencia;
                    DateTime fechaActual = DateTime.Now;
                    fechaDeVenta = fechaActual.ToString("dd/MM/yyyy");
                    nombreProducto = respuesta.Producto.Nombre;
                    detalleProducto = respuesta.Producto.Detalle;
                    precioProducto = respuesta.Producto.PrecioDeVenta;
                    totalProducto = int.Parse(textTotalFacturaGenerada.Text);
                    ProductoVendidoTxt productoVendidoTxt = new ProductoVendidoTxt(fechaDeVenta, cantidadProducto, referenciaProducto, nombreProducto, detalleProducto, precioProducto, totalProducto);
                    string mensaje = productoVendidoTxtService.Guardar(productoVendidoTxt, rutasVendidos);
                }
                else
                {
                    if (contadorProductosVendidos > 1)
                    {
                        referenciaProducto = respuesta.Producto.Referencia;
                        DateTime fechaActual = DateTime.Now;
                        fechaDeVenta = fechaActual.ToString("dd/MM/yyyy");
                        nombreProducto = respuesta.Producto.Nombre;
                        detalleProducto = respuesta.Producto.Detalle;
                        var producto = productoTxtService.FiltroProducto(referenciaProducto);
                        precioProducto = producto.Precio;
                        totalProducto = cantidadProducto * precioProducto;
                        ProductoVendidoTxt productoVendidoTxt = new ProductoVendidoTxt(fechaDeVenta, cantidadProducto, referenciaProducto, nombreProducto, detalleProducto, precioProducto, totalProducto);
                        string mensaje = productoVendidoTxtService.Guardar(productoVendidoTxt, rutasVendidos);
                    }
                }
            }
        }
        private void ContarProductosVendidos()
        {
            contadorProductosVendidos = 0;
            foreach (DataGridViewRow fila in dataGridFacturaProductos.Rows)
            {
                contadorProductosVendidos = contadorProductosVendidos + 1;
            }
        }
        private void ContarCantidadesProductosVendidos()
        {
            ContarProductosVendidos();
            foreach (DataGridViewRow fila in dataGridFacturaProductos.Rows)
            {
                int i = 0;
                foreach (DataGridViewCell celda in fila.Cells)
                {
                    if (i == 1)
                    {
                        cantidadProducto = Convert.ToInt32(fila.Cells[i].Value);
                    }
                    else
                    {
                        if (i == 2)
                        {
                            referencias[i] = Convert.ToString(fila.Cells[i].Value);
                            referencia = referencias[i];
                            MapearProductosVendidos(referencia);
                            break;
                        }
                    }
                    i = i + 1;
                }
            }
        }
        private void MapearDatosActualesDeFactura(Factura factura)
        {
            factura.GenerarFechaYHoraFactura();
            fechaFactura = factura.FechaHora;
            nombreEmpleado = factura.NombreDeEmpleado;
            ciudad = factura.Ciudad;
            nombreCliente = factura.NombreDeCliente;
            totalSinRedondeo = factura.TotalSinRedondeo;
            factura.TotalizarFactura();
            totalConRedondeo = factura.TotalConRedondeo;
            factura.TotalFactura = int.Parse(textTotalFacturaGenerada.Text);
            totalFactura = factura.TotalFactura;
            valorDeRedondeo = factura.ValorDeRedondeo;
            formaDePago = factura.FormaDePago;
        }
        private void SumatoriaDeFactura()
        {
            if (dataGridFacturaProductos != null)
            {
                totalFactura = 0;
                foreach (DataGridViewRow fila in dataGridFacturaProductos.Rows)
                {
                    int i = 0;
                    int cantidad = 0;
                    foreach (DataGridViewCell celda in fila.Cells)
                    {
                        //Determinamos la cantidad del producto
                        if (i == 1)
                        {
                            cantidad = Convert.ToInt32(fila.Cells[i].Value);
                            cantidadARestar = cantidad;
                        }
                        else
                        {
                            //Determinamos el valor por unidad y luego multiplicamos por la cantidad
                            if (i == 5)
                            {
                                int valorUnidad = Convert.ToInt32(fila.Cells[i].Value);
                                int valorTotal = valorUnidad * cantidad;
                                totalFactura = totalFactura + valorTotal;
                            }
                        }
                        i = i + 1;
                    }
                }
                textTotalFacturaGenerada.Text = totalFactura.ToString();
            }
        }
        public void ConsultarCajaAbierta()
        {
            BusquedaCajaRegistradoraRespuesta respuesta = new BusquedaCajaRegistradoraRespuesta();
            string estado = "Abierta";
            respuesta = cajaRegistradoraService.BuscarPorEstado(estado);
            if (respuesta.CajaRegistradora != null)
            {
                var cajasRegistradoras = new List<Caja> { respuesta.CajaRegistradora };
                labelCash.Text = respuesta.CajaRegistradora.VentaDelDia.ToString();
                labelBase.Text = respuesta.CajaRegistradora.MontoInicial.ToString();
                idCajaAbierta = respuesta.CajaRegistradora.IdCaja;
                montoActualCaja = respuesta.CajaRegistradora.MontoFinal;
                montoActualVenta = respuesta.CajaRegistradora.VentaDelDia;
            }
            else
            {
                if (respuesta.CajaRegistradora == null)
                {
                    labelCash.Text = "Sin definir";
                    labelBase.Text = "Sin definir";
                }
            }
        }
        private Factura GenerarIdFactura()
        {
            factura = new Factura();
            factura.GenerarIdFactura();
            return factura;
        }
        private void SecuenciaDeFactura()
        {
            for (int i = 1; i <= 1000; i++)
            {
                BusquedaFacturaRespuesta respuesta = new BusquedaFacturaRespuesta();
                respuesta = facturaService.BuscarPorSecuencia(i);
                if (respuesta.Factura != null)
                {
                    secuenciaDeFactura = secuenciaDeFactura + 1;
                }
                else
                {
                    if (respuesta.Factura == null)
                    {
                        secuenciaDeFactura = i;
                        break;
                    }
                }
            }
        }
        private void CargarArchivo(ProductoFacturaTxtService productoTxtService)
        {
            Factura factura = GenerarIdFactura();
            id_factura = factura.Id_Factura;
            labelIdGeneradoDeFactura.Text = id_factura;
            ProductoFacturaTxtConsultaResponse productoTxtConsultaResponse = productoTxtService.Consultar();
            if (productoTxtConsultaResponse.ProductoTxts.Count > 0)
            {
                foreach (var item in productoTxtConsultaResponse.ProductoTxts)
                {
                    Deshacer.Image = Properties.Resources.Regresar;
                    cantidadProducto = item.Cantidad;
                    referenciaProducto = item.Referencia;
                    nombreProducto = item.Nombre;
                    detalleProducto = item.Detalle;
                    precioProducto = item.Precio;
                    dataGridFacturaProductos.Rows.Add(Deshacer.Image, cantidadProducto, referenciaProducto, nombreProducto, detalleProducto, precioProducto);
                }
            }
            else
            {
                if (productoTxtConsultaResponse.ProductoTxts.Count == 0)
                {
                    dataGridFacturaProductos = null;
                }
            }
        }
        private void ConsultarHistorial()
        {
            ProductoFacturaTxtConsultaResponse productoTxtConsultaResponse = productoTxtService.Consultar();
            if (productoTxtConsultaResponse.Encontrado == true)
            {
                foreach (var item in productoTxtConsultaResponse.ProductoTxts)
                {
                    Deshacer.Image = Properties.Resources.Regresar;
                    int cantidad = item.Cantidad;
                    string referencia = item.Referencia;
                    string nombre = item.Nombre;
                    string detalle = item.Detalle;
                    double precio = item.Precio;
                    dataGridFacturaProductos.Rows.Add(Deshacer.Image, cantidad, referencia, nombre, detalle, precio);
                }
            }
            else
            {
                string mensaje = productoTxtConsultaResponse.Mensaje;
                MessageBox.Show(mensaje.ToString());
            }
        }
        private void EliminarFactura()
        {
            ProductoFacturaTxt productoTxt = new ProductoFacturaTxt();
            string mensaje = productoTxtService.EliminarHistorial();
            ConsultarHistorial();
        }
        private Caja MapearCashCaja()
        {
            cajaRegistradora = new Caja();
            cajaRegistradora.IdCaja = idCajaAbierta;
            cajaRegistradora.VentaDelDia = montoActualVenta + totalFactura;
            double totalMonto = montoActualCaja + totalFactura;
            cajaRegistradora.MontoFinal = totalMonto;
            return cajaRegistradora;
        }
        private void ModificarCashCaja()
        {
            Caja cajaRegistradora = MapearCashCaja();
            cajaRegistradoraService.ModificarCash(cajaRegistradora);
            labelCash.Text = "Sin definir";
            labelBase.Text = "Sin definir";
        }
        private void ConsultarDatosGimnasio()
        {
            BusquedaGimnasioRespuesta respuesta = new BusquedaGimnasioRespuesta();
            string id_Gimnasio = "#Drog";
            respuesta = gimnasioService.BuscarPorId(id_Gimnasio);
            if (respuesta.Gimnasio != null)
            {
                var gimnasios = new List<Gimnasio> { respuesta.Gimnasio };
                var gimnasio = respuesta.Gimnasio;
                nombreGimnasio = gimnasio.NombreGimnasio;
                codigoCamara = gimnasio.CodigoDeCamara;
            }
            else
            {
                if (respuesta.Gimnasio == null)
                {
                    labelCash.Text = "Sin definir";
                }
            }
        }
        private void BuscararGimnasio()
        {
            BusquedaGimnasioRespuesta respuesta = new BusquedaGimnasioRespuesta();
            respuesta = gimnasioService.BuscarPorId(idGimnasio);
            if (respuesta.Gimnasio != null)
            {
                nombreGimnasio = respuesta.Gimnasio.NombreGimnasio;
                nitGimnasio = respuesta.Gimnasio.NIT;
                fraseDistintiva = respuesta.Gimnasio.FraseDistintiva;
                regimen = respuesta.Gimnasio.Regimen;
                pbx = respuesta.Gimnasio.PBX;
                direccion = respuesta.Gimnasio.Direccion;
                telefono = respuesta.Gimnasio.Telefono;
            }
            else
            {
                if (respuesta.Gimnasio == null)
                {

                }
            }
        }
        private Factura MapearFactura()
        {
            factura = new Factura();
            //Mapeamos Datos de Gimnasio
            factura.Id_Factura = id_factura;
            //Mapeamos Datos de factura
            SecuenciaDeFactura();
            factura.SecuenciaDeFactura = secuenciaDeFactura;
            factura.NombreDeEmpleado = textNombreEmpleado.Text;
            factura.Ciudad = "Valledupar, Cesar";
            factura.IdCaja = idCajaAbierta;
            factura.NombreDeCliente = nombreCliente;
            factura.TotalSinRedondeo = 0;
            //Mapeamos Forma de pago
            factura.FormaDePago = comboFormaDePago.Text;
            return factura;
        }
        private void ImprimirFactura()
        {
            //Proceso de impresion
            string nombreFactura = id_factura + ".pdf";
            nombreDeFactura = nombreFactura;
            string existingPathName = @"" + rutaTxtFacturaVenta;
            string notExistingFileName = rutaTxtFacturaVenta + "\\" + nombreFactura;
            notExistFileName = notExistingFileName;

            if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
            {
                ImprimirDocumento = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                Margins margins = new Margins(0, 20, 20, 20);

                ImprimirDocumento.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                ImprimirDocumento.PrinterSettings.PrintFileName = notExistingFileName;
                ImprimirDocumento.PrinterSettings.PrintToFile = true;
                ImprimirDocumento.PrinterSettings = ps;
                ImprimirDocumento.DefaultPageSettings.Margins = margins;
                ImprimirDocumento.PrintPage += Imprimir;
                ImprimirDocumento.Print();
            }
        }
        private void GuardarFactura()
        {
            //Proceso de impresion
            string nombreFactura = id_factura + ".pdf";
            nombreDeFactura = nombreFactura;
            string existingPathName = @"" + rutaTxtFacturaVenta;
            string notExistingFileName = rutaTxtFacturaVenta + "\\" + nombreFactura;
            notExistFileName = notExistingFileName;

            if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
            {
                ImprimirDocumento = new PrintDocument();
                PrinterSettings ps = new PrinterSettings();
                Margins margins = new Margins(0, 20, 20, 20);

                ImprimirDocumento.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                ImprimirDocumento.PrinterSettings.PrintFileName = notExistingFileName;
                ImprimirDocumento.PrinterSettings.PrintToFile = true;
                ImprimirDocumento.DefaultPageSettings.Margins = margins;
                ImprimirDocumento.PrintPage += Imprimir;
                ImprimirDocumento.Print();
                ImprimirFactura();
            }
        }

        //*************************************************Eventos*****************************************************
        private void textPago_TextChanged(object sender, EventArgs e)
        {
            if (textPago.Text != "")
            {
                labelVueltosGenerado.Text = "";
                int pago = int.Parse(textPago.Text);
                int TotalFactura = int.Parse(textTotalFacturaGenerada.Text);
                int diferencia = pago - TotalFactura;
                labelVueltosGenerado.Text = diferencia.ToString();
            }
        }
        private void textSearchEmpleado_Enter(object sender, EventArgs e)
        {
            if (textSearchEmpleado.Text == "Buscar identificacion")
            {
                textSearchEmpleado.Text = "";
            }
        }
        private void textSearchCliente_Enter(object sender, EventArgs e)
        {
            if (textSearchCliente.Text == "Buscar identificacion")
            {
                textSearchCliente.Text = "";
            }
        }
        private void textSearchEmpleado_TextChanged(object sender, EventArgs e)
        {
            if (textSearchEmpleado.Text != "")
            {
                BusquedaEmpleadoRespuesta respuesta = new BusquedaEmpleadoRespuesta();
                string Id_Empleado = textSearchEmpleado.Text;
                respuesta = empleadoService.BuscarPorIdentificacion(Id_Empleado);
                if (respuesta.Empleado != null)
                {
                    labelAdvertenciaEmpleado.Visible = false;
                    var empleados = new List<Empleado> { respuesta.Empleado };
                    var empleado = respuesta.Empleado;
                    textNombreEmpleado.Text = empleado.Nombres;
                    textApellidoEmpleado.Text = empleado.Apellidos;
                    textIdentificacionEmpleado.Text = empleado.Identificacion;
                }
                else
                {
                    if (respuesta.Empleado == null)
                    {
                        labelAdvertenciaEmpleado.Visible = true;
                        textNombreEmpleado.Text = "";
                        textApellidoEmpleado.Text = "";
                        textIdentificacionEmpleado.Text = "";
                    }
                }
            }
        }

        private void textSearchCliente_TextChanged(object sender, EventArgs e)
        {
            if (textSearchCliente.Text != "")
            {
                BusquedaClienteRespuesta respuesta = new BusquedaClienteRespuesta();
                string Id_Cliente = textSearchCliente.Text;
                respuesta = clienteService.BuscarPorIdentificacion(Id_Cliente);
                if (respuesta.Cliente != null)
                {
                    labelAdvertenciaCliente.Visible = false;
                    var clientes = new List<Cliente> { respuesta.Cliente };
                    var cliente = respuesta.Cliente;
                    textNombreCliente.Text = cliente.Nombres;
                    nombreCliente = textNombreCliente.Text;
                    textApellidoCliente.Text = cliente.Apellidos;
                    textIdentificacionCliente.Text = cliente.Identificacion;
                    comboTipoDeId.Text = cliente.TipoDeIdentificacion;
                    comboSexo.Text = cliente.Sexo;
                    dateTimeFechaDeNacimiento.Value = cliente.FechaDeNacimiento;
                    textDireccion.Text = cliente.Direccion;
                    textTelefono.Text = cliente.Telefono;
                    textCorreo.Text = cliente.CorreoElectronico;
                }
                else
                {
                    if (respuesta.Cliente == null)
                    {
                        labelAdvertenciaCliente.Visible = true;
                        textNombreCliente.Text = "";
                        textApellidoCliente.Text = "";
                        textIdentificacionCliente.Text = "";
                        comboTipoDeId.Text = "";
                        comboSexo.Text = "";
                        dateTimeFechaDeNacimiento.Value = DateTime.Now;
                        textDireccion.Text = "";
                        textTelefono.Text = "";
                        textCorreo.Text = "";
                    }
                }
            }
        }

        private void textPago_TextChanged_1(object sender, EventArgs e)
        {
            if (textPago.Text != "")
            {
                double totalFactura = double.Parse(textTotalFacturaGenerada.Text);
                double pago = double.Parse(textPago.Text);
                labelVueltosGenerado.Text = (pago - totalFactura).ToString();
            }
        }
        //******************************************Administrar factura**************************************************
        private void Imprimir(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Arial Narrow", 8);
            int ancho = 220;
            int y = 20;
            StringFormat stringFormatCenter = new StringFormat();
            stringFormatCenter.Alignment = StringAlignment.Center;
            stringFormatCenter.LineAlignment = StringAlignment.Center;

            StringFormat stringFormatRight = new StringFormat();
            stringFormatRight.Alignment = StringAlignment.Far;
            stringFormatRight.LineAlignment = StringAlignment.Far;

            e.Graphics.DrawString(nombreGimnasio, font, Brushes.Black, new RectangleF(0, y, ancho, 20), stringFormatCenter);

            e.Graphics.DrawString("NIT: " + nitGimnasio, font, Brushes.Black, new RectangleF(0, y + 40, ancho, 13), stringFormatCenter);
            e.Graphics.DrawString("Actividad economica: " + codigoCamara, font, Brushes.Black, new RectangleF(0, y + 53, ancho, 13), stringFormatCenter);
            e.Graphics.DrawString(fraseDistintiva, font, Brushes.Black, new RectangleF(0, y + 66, ancho, 13), stringFormatCenter);
            e.Graphics.DrawString("PBX: " + pbx, font, Brushes.Black, new RectangleF(0, y + 79, ancho, 13), stringFormatCenter);
            e.Graphics.DrawString("Regimen: " + regimen, font, Brushes.Black, new RectangleF(0, y + 92, ancho, 13), stringFormatCenter);
            e.Graphics.DrawString("Direccion: " + direccion, font, Brushes.Black, new RectangleF(0, y + 105, ancho, 13), stringFormatCenter);
            e.Graphics.DrawString("Telefono: " + telefono, font, Brushes.Black, new RectangleF(0, y + 118, ancho, 13), stringFormatCenter);

            e.Graphics.DrawString("Id factura: " + id_factura, font, Brushes.Black, new RectangleF(0, y + 143, ancho, 13));
            e.Graphics.DrawString("Secuencia: " + secuenciaDeFactura, font, Brushes.Black, new RectangleF(0, y + 153, ancho, 13));
            e.Graphics.DrawString("FechaYhora: " + fechaFactura, font, Brushes.Black, new RectangleF(0, y + 166, ancho, 13));
            e.Graphics.DrawString("Empleado: " + nombreEmpleado, font, Brushes.Black, new RectangleF(0, y + 179, ancho, 13));
            e.Graphics.DrawString("Ciudad: " + ciudad, font, Brushes.Black, new RectangleF(0, y + 192, ancho, 13));
            e.Graphics.DrawString("Cliente: " + nombreCliente, font, Brushes.Black, new RectangleF(0, y + 205, ancho, 13));
            e.Graphics.DrawString("IdDeCaja: " + idCajaAbierta, font, Brushes.Black, new RectangleF(0, y + 218, ancho, 13));

            e.Graphics.DrawString("FACTURA DE VENTA", font, Brushes.Black, new RectangleF(0, y + 244, ancho, 14));

            e.Graphics.DrawString("Lista de productos", font, Brushes.Black, new RectangleF(0, y + 270, ancho, 14));
            e.Graphics.DrawString(" Cantidad " + " Nombre " + " Detalle " + " Precio ", font, Brushes.Black, new RectangleF(0, y + 284, ancho, 14));
            int r = 0;
            int j = 298;
            foreach (DataGridViewRow fila in dataGridFacturaProductos.Rows)
            {
                int i = 0;
                foreach (DataGridViewCell celda in fila.Cells)
                {
                    e.Graphics.DrawString("    " + Convert.ToString(fila.Cells[i + 1].Value) + " " + Convert.ToString(fila.Cells[i + 3].Value) + " " + Convert.ToString(fila.Cells[i + 4].Value) + " " + Convert.ToString(fila.Cells[i + 5].Value), font, Brushes.Black, new RectangleF(0, y + j, ancho, 14));
                    j = j + 14;
                    int x = y + j;
                    r = x;
                    break;
                }
            }
            e.Graphics.DrawString("Valor Total: " + totalFactura, font, Brushes.Black, new RectangleF(-30, r + 30, ancho, 14), stringFormatRight);
            e.Graphics.DrawString("Vueltos: " + labelVueltosGenerado.Text, font, Brushes.Black, new RectangleF(-30, r + 44, ancho, 14), stringFormatRight);
            e.Graphics.DrawString("Forma de pago: " + comboFormaDePago.Text, font, Brushes.Black, new RectangleF(-30, r + 58, ancho, 14), stringFormatRight);

            e.Graphics.DrawString("!Gracias por su compra! ", font, Brushes.Black, new RectangleF(-20, r + 84, ancho, 14), stringFormatCenter);
            e.Graphics.DrawString("     Vuelva pronto     ", font, Brushes.Black, new RectangleF(-20, r + 98, ancho, 14), stringFormatCenter);
        }
        //*************************************************Botones*****************************************************
        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnSearchEmpleado_Click(object sender, EventArgs e)
        {
            textSearchEmpleado.Visible = true;
            btnCloseEmpleado.Visible = true;
        }
        private void btnCloseEmpleado_Click(object sender, EventArgs e)
        {
            textSearchEmpleado.Visible = false;
            btnCloseEmpleado.Visible = false;
        }

        private void btnSearchCliente_Click(object sender, EventArgs e)
        {
            textSearchCliente.Visible = true;
            btnCloseCliente.Visible = true;
        }
        private void btnCloseCliente_Click(object sender, EventArgs e)
        {
            textSearchCliente.Visible = false;
            btnCloseCliente.Visible = false;
        }
        private void ObtenerRutaDeGuardado()
        {
            RutasTxtConsultaResponse rutasTxtConsultaResponse = rutasTxtService.Consultar();
            if (rutasTxtConsultaResponse.RutasTxts.Count > 0)
            {
                foreach (var item in rutasTxtConsultaResponse.RutasTxts)
                {
                    rutaTxtFacturaVenta = item.RutaFacturasVenta;
                }
            }
        }
        private void btnImprimirFactura_Click(object sender, EventArgs e)
        {
            ObtenerRutaDeGuardado();
            Factura factura = MapearFactura();
            MapearDatosActualesDeFactura(factura);
            facturaService.Guardar(factura);
            ContarCantidadesProductosVendidos();
            ModificarCashCaja();
            ConsultarCajaAbierta();
            GuardarFactura();
            EliminarFactura();
            FormVisorDeFactura frm = new FormVisorDeFactura();
            frm.nombreDeArchivo = nombreDeFactura;
            frm.rutaDeGuardado = notExistFileName;
            frm.ShowDialog();
            this.Close();
        }
        private void btnSoloVender_Click(object sender, EventArgs e)
        {
            Factura factura = MapearFactura();
            MapearDatosActualesDeFactura(factura);
            facturaService.Guardar(factura);
            ContarCantidadesProductosVendidos();
            ModificarCashCaja();
            ConsultarCajaAbierta();
            EliminarFactura();
            this.Close();
        }
        private void menuTop_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void FormFacturaDeProducto_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void textPago_Enter(object sender, EventArgs e)
        {
            if (textPago.Text == "0.00")
            {
                textPago.Text = "";
            }
        }

        private void textPago_Leave(object sender, EventArgs e)
        {
            if (textPago.Text == "")
            {
                textPago.Text = "0.00";
            }
        }
        private void DevolverAlInventario(string referencia, int cantidad)
        {
            BusquedaProductoRespuesta respuesta = new BusquedaProductoRespuesta();
            respuesta = productoService.BuscarPorReferencia(referencia);
            if (respuesta.Producto != null)
            {
                cantidadProductoBD = respuesta.Producto.Cantidad;
                respuesta.Producto.Cantidad = cantidadProductoBD + cantidad;
                var producto = respuesta.Producto;
                productoService.ModificarCantidad(producto);
                productoTxtService.Eliminar(referencia);
                productoVendidoTxtService.Eliminar(referencia, rutasVendidos);
                dataGridFacturaProductos.Rows.Clear();
            }
        }
        private void dataGridFacturaProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string referencia;
            string nombre;
            int cantidad;
            if (dataGridFacturaProductos.Columns[e.ColumnIndex].Name == "Deshacer")
            {
                referencia = Convert.ToString(dataGridFacturaProductos.CurrentRow.Cells["ReferenciaP"].Value.ToString());
                nombre = Convert.ToString(dataGridFacturaProductos.CurrentRow.Cells["Nombre"].Value.ToString());
                cantidad = Convert.ToInt32(dataGridFacturaProductos.CurrentRow.Cells["Cantidad"].Value);
                string msg = "Desea regresar el producto " + nombre + " al inventario?";
                var respuesta = MessageBox.Show(msg, "Deshacer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (respuesta == DialogResult.OK)
                {
                    DevolverAlInventario(referencia, cantidad);
                    ConsultarHistorial();
                }
            }
        }

        private void dataGridFacturaProductos_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ContarProductosVendidos();
            int valor = Convert.ToInt32(dataGridFacturaProductos.CurrentRow.Cells["Precio"].Value);
            string referencia = Convert.ToString(dataGridFacturaProductos.CurrentRow.Cells["ReferenciaP"].Value);
            var productoMapeado = productoTxtService.FiltroProducto(referencia);
            if (productoMapeado != null)
            {
                productoMapeado.Precio = valor;
                productoTxtService.Modificar(productoMapeado, referencia);
            }
            SumatoriaDeFactura();
        }

        private void dataGridFacturaProductos_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //empezo
        }
    }
}
