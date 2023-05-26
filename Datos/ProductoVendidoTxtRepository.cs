﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using System.IO;

namespace Datos
{
    public class ProductoVendidoTxtRepository
    {
        private string ruta;
        private string NombreArchivo = @"ProductosVendidos.txt";
        public void Guardar(ProductoVendidoTxt productoTxt, string rutasVendidos)
        {
            ruta = @"" + rutasVendidos + "\\" + NombreArchivo;
            FileStream file = new FileStream(ruta, FileMode.Append);
            StreamWriter escritor = new StreamWriter(file);
            escritor.WriteLine($"{productoTxt.FechaDeVenta};{productoTxt.Cantidad};{productoTxt.Referencia};{productoTxt.Nombre};{productoTxt.Detalle};{productoTxt.Precio};{productoTxt.Total}");
            escritor.Close();
            file.Close();
        }
        public List<ProductoVendidoTxt> Consultar(string rutasVendidos)
        {
            ruta = @"" + rutasVendidos + "\\" + NombreArchivo;
            List<ProductoVendidoTxt> productoTxts = new List<ProductoVendidoTxt>();
            FileStream file = new FileStream(ruta, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader lector = new StreamReader(ruta);
            var linea = "";
            while ((linea = lector.ReadLine()) != null)
            {
                string[] dato = linea.Split(';');
                ProductoVendidoTxt productoTxt = new ProductoVendidoTxt()
                {
                    FechaDeVenta=dato[0],
                    Cantidad = int.Parse(dato[1]),
                    Referencia = dato[2],
                    Nombre = dato[3],
                    Detalle = dato[4],
                    Precio = int.Parse(dato[5]),
                    Total=int.Parse(dato[6])
                };
                productoTxts.Add(productoTxt);
            }
            lector.Close();
            file.Close();
            return productoTxts;
        }
        public bool FiltroIdentificaicon(string referencia, string rutasVendidos )
        {
            ruta = @"" + rutasVendidos + "\\" + NombreArchivo;
            List<ProductoVendidoTxt> productoTxts = new List<ProductoVendidoTxt>();
            FileStream file = new FileStream(ruta, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader lector = new StreamReader(ruta);
            var linea = "";
            while ((linea = lector.ReadLine()) != null)
            {
                string[] dato = linea.Split(';');
                if (dato[1].Equals(referencia))
                {
                    lector.Close();
                    file.Close();
                    return true;
                }
            }
            lector.Close();
            file.Close();
            return false;
        }
        public List<ProductoVendidoTxt> ConsultarPorFechas(string referencia, string rutasVendidos)
        {
            ruta = @"" + rutasVendidos + "\\" + NombreArchivo;
            List<ProductoVendidoTxt> productoTxts = new List<ProductoVendidoTxt>();
            FileStream file = new FileStream(ruta, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader lector = new StreamReader(ruta);
            var linea = "";
            while ((linea = lector.ReadLine()) != null)
            {
                string[] dato = linea.Split(';');
                if (dato[0].Equals(referencia))
                {
                    ProductoVendidoTxt productoTxt = new ProductoVendidoTxt()
                    {
                        FechaDeVenta = dato[0],
                        Cantidad = int.Parse(dato[1]),
                        Referencia = dato[2],
                        Nombre = dato[3],
                        Detalle = dato[4],
                        Precio = int.Parse(dato[5]),
                        Total= int.Parse(dato[6]),
                    };
                    productoTxts.Add(productoTxt);
                }
            }
            lector.Close();
            file.Close();
            return productoTxts;
        }
        private bool EsEncontrado(string referenciaRegistrada, string referenciaBuscada)
        {
            return referenciaRegistrada == referenciaBuscada;
        }
        public void Modificar(ProductoVendidoTxt productoTxt, string referencia, string rutasVendidos)
        {
            ruta = @"" + rutasVendidos + "\\" + NombreArchivo;
            List<ProductoVendidoTxt> productoTxts = new List<ProductoVendidoTxt>();
            productoTxts = Consultar(rutasVendidos);
            FileStream file = new FileStream(ruta, FileMode.Create);
            file.Close();
            foreach (var item in productoTxts)
            {
                if (!EsEncontrado(item.Referencia, referencia))
                {
                    Guardar(item, rutasVendidos);
                }
                else
                {
                    Guardar(productoTxt, rutasVendidos);
                }
            }
        }
        public void EliminarTodo()
        {
            File.Delete(ruta);
        }
        public void Eliminar(string referencia, string rutasVendidos)
        {
            ruta = @"" + rutasVendidos + "\\" + NombreArchivo;
            List<ProductoVendidoTxt> productoTxts = Consultar(rutasVendidos);
            FileStream file = new FileStream(ruta, FileMode.Create);
            file.Close();

            foreach (var item in productoTxts)
            {
                if (!item.Referencia.Equals(referencia))
                {
                    Guardar(item, rutasVendidos);
                }
            }
        }
        public int Totalizar(string rutasVendidos)
        {
            return Consultar(rutasVendidos).Count();
        }
    }
}
