using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;

namespace vaca2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        double posicionAgua2RespectoAgua1 = 30;
        double posicionVacaRespectoAgua1 = 90;

        double posicionTopAgua1 = 0;
        double posicionTopOriginalVaca = 264;
        double altoCanvas = 0;

        List<Image> gotasLluvia = null;
        
        public MainWindow()
        {
            gotasLluvia = new List<Image>();
            InitializeComponent();
            altoCanvas = this.CanvasDiaLluvioso.Height;

            txtIndice.TextChanged += txtIndice_TextChanged;
            txtIndice.PreviewKeyDown += txtIndice_PreviewKeyDown;

            GenerarGotasLluvia();
        }

        // Genera las gotas de lluvia pero no se encontrarán visibles 
        private void GenerarGotasLluvia()
        {
            Storyboard sbLluvia = FindResource("sbLluvia") as Storyboard;

                for (int i = 0; i < 120; i++)
                {
                    Image nuevaImagen = new Image();
                    nuevaImagen.Source = new BitmapImage(new Uri(@"lluvia.png", UriKind.Relative));
                    nuevaImagen.Name = "gota" + i.ToString();

                    this.CanvasDiaLluvioso.Children.Add(nuevaImagen);
                    Canvas.SetLeft(nuevaImagen, -30);
                    Canvas.SetTop(nuevaImagen, -30);
                    Canvas.SetZIndex(nuevaImagen, 10);

                    gotasLluvia.Add(nuevaImagen);
                }
        }
        
        // Cada vez que esté lloviendo y cambie el valor del textbox cambiará
        // la animación del agua y la vaca y la velocidad de la lluvia
        private void txtIndice_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Solicita animar los componentes si se ha ingresado un
            // valor numérico y es válido
            if (chkEstaLloviendo.IsChecked == true)
            {
                if (ValidarValorIngresado())
                {
                    double valorIngresado = Convert.ToDouble(this.txtIndice.Text);
                    AnimarAguaYVaca(Convert.ToInt32(valorIngresado));
                    CambiarVelocidadLluvia();
                }
            }
        }

        // Cada vez que el usuario presiona la tecla del teclado de 
        // arriba o abajo se incrementa o decrementa el valor del indice en 1
        private void txtIndice_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down ||e.Key == Key.Up)
            {
                if (this.chkEstaLloviendo.IsChecked == true)
                {
                    if (ValidarValorIngresado())
                    {
                        double valorIngresado = Convert.ToDouble(this.txtIndice.Text);

                        if (e.Key == Key.Down) { valorIngresado--; }
                        if (e.Key == Key.Up) { valorIngresado++; }

                        if (valorIngresado < 0) { valorIngresado = 0; }
                        if (valorIngresado > 9) { valorIngresado = 9; }

                        this.txtIndice.Text = valorIngresado.ToString();
                    }
                }
            }
        }

        // Muestra un mensaje de error e indica si el usuario ha cambiado el indice
        // y el valor ingresado no sea válido
        private bool ValidarValorIngresado()
        {
            bool valorIngresadoValido = false;
            if (Regex.IsMatch(this.txtIndice.Text, @"^\d$"))
            {
                double indice = Convert.ToDouble(this.txtIndice.Text);

                if (indice >= 0 && indice <= 9)
                {
                    valorIngresadoValido = true;
                }
                else
                {
                    MessageBox.Show("Debe ingresar un valor entre 0 y 9");
                }
            }
            else
            {
                MessageBox.Show("Debe ingresar un valor numérico");
            }

            return valorIngresadoValido;
        }

        // Si está lloviendo inicia la animación de la lluvia y mueve 
        // la vaca y el agua como consecuecia de la lluvia
        private void chkEstaLloviendo_Checked(object sender, RoutedEventArgs e)
        {
            if (this.chkEstaLloviendo.IsChecked == true)
            {
                int indiceIngresado = Convert.ToInt32(this.txtIndice.Text);
                this.AnimarLluvia(indiceIngresado);
                this.AnimarAguaYVaca(indiceIngresado);
            }
            else
            {
                DetenerAnimacionLluvia();
            }
        }

        // Anima la caída de la lluvia
        private void AnimarLluvia(int indiceIngresado)
        {
            Storyboard sbLluvia = FindResource("sbLluvia") as Storyboard;

            // Aparecer la nube de la lluvia
            this.Nube.Visibility = Visibility.Visible;
            this.AparecerImagen(sbLluvia, this.Nube);

            // Reposicionar las gotas de lluvia
            foreach (Image gota in gotasLluvia)
            {
                Canvas.SetLeft(gota, -30);
                Canvas.SetTop(gota, -30);
                Canvas.SetZIndex(gota, 10);
            }

            // Animar las gotas de agua
            double segundoInicioGota;
            double posicionHorizontal;
            Random numeros = new Random();

            foreach (Image imagenGota in gotasLluvia)
            {
                // Desplazamiento vertical
                segundoInicioGota = numeros.NextDouble();
                segundoInicioGota += numeros.Next(0, 2);

                this.DesplazarImagen(sbLluvia, imagenGota, -20, CanvasDiaLluvioso.Height, "Canvas.Top", 0, segundoInicioGota, 1, true);

                // Posicion horizontal
                posicionHorizontal = numeros.Next(0, Convert.ToInt32(this.CanvasDiaLluvioso.Width));
                this.DesplazarImagen(sbLluvia, imagenGota, posicionHorizontal, posicionHorizontal, "Canvas.Left");

                this.AparecerImagen(sbLluvia, imagenGota);
            }

            // Inicia la animación
            IniciarAnimacionLluvia();
            
            // Cambia la velocidad de la lluvia dependiendo
            // del valor del indice ingresado
            CambiarVelocidadLluvia();
        }

        // Anima la elevación o caía de la posición vertical
        // del agua y la vaca
        private void AnimarAguaYVaca(int indiceIngresado)
        {
            double posicionVerticalAgua1 = this.ObtenerPosicionVerticalAgua(indiceIngresado);

            double posicionHorizontalAgua1 = 0;
            double posicionHorizontalAgua2 = 0;

            if (Canvas.GetLeft(this.Agua1) != 0)
            {
                posicionHorizontalAgua1 = -163;
            }

            if (Canvas.GetLeft(this.Agua2) != 0)
            {
                posicionHorizontalAgua2 = -163;
            }

            double posicionVerticalVaca = posicionTopOriginalVaca;

            this.Vaca.Source = new BitmapImage(new Uri(@"vaca1.png", UriKind.Relative));
            if (posicionVerticalAgua1 <= 350)
            {
                posicionVerticalVaca = posicionVerticalAgua1 - posicionVacaRespectoAgua1;
                this.Vaca.Source = new BitmapImage(new Uri(@"vaca_agu.png", UriKind.Relative));
            }

            Storyboard sbConsecuenciasLluvia = FindResource("sbConsecuenciasLluvia") as Storyboard;

            DesplazarImagen(sbConsecuenciasLluvia, this.Agua1, Canvas.GetTop(this.Agua1), posicionVerticalAgua1, "Canvas.Top", 0.5);
            DesplazarImagen(sbConsecuenciasLluvia, this.Agua1, Canvas.GetLeft(this.Agua1), posicionHorizontalAgua2, "Canvas.Left", 0.5);

            DesplazarImagen(sbConsecuenciasLluvia, this.Agua2, Canvas.GetTop(this.Agua2), posicionVerticalAgua1 - posicionAgua2RespectoAgua1, "Canvas.Top", 0.5);
            DesplazarImagen(sbConsecuenciasLluvia, this.Agua2, Canvas.GetLeft(this.Agua2), posicionHorizontalAgua1, "Canvas.Left", 0.5);

            DesplazarImagen(sbConsecuenciasLluvia, this.Vaca, Canvas.GetTop(this.Vaca), posicionVerticalVaca, "Canvas.Top", 0.75);

            IniciarAnimacionConsecuenciaLluvia();
        }

        // Posicion en la que debe estar el agua segun el indice ingresado
        private double ObtenerPosicionVerticalAgua(int indiceIngresado)
        {
            double porcentajeAgua = indiceIngresado * 10;
            double posicionAgua = (altoCanvas * porcentajeAgua) / 100;
            posicionAgua = altoCanvas - posicionAgua;
            return posicionAgua;
        }

        // desplaza una imagen desde un punto indicado hacia otro
        // dependiendo de la propiedad del canvas indicada 
        private void DesplazarImagen(Storyboard storyboard, Image imagen, double puntoInicial, double puntoFinal, string propiedadCanvas, double duracion = 0,
                                        double segundoInicial = 0, double velocidad = 1, bool cicloInfinito = false)
        {
            DoubleAnimation desplazamiento = new DoubleAnimation();
            desplazamiento.From = puntoInicial;
            desplazamiento.To = puntoFinal;
            
            if (duracion != 0)
            {
                desplazamiento.Duration = new Duration(TimeSpan.FromSeconds(duracion));
            }

            desplazamiento.BeginTime = TimeSpan.FromSeconds(segundoInicial);
            desplazamiento.SpeedRatio = 1;

            // Agregar el desplazamiento al storyline
            storyboard.Children.Add(desplazamiento);
            Storyboard.SetTarget(desplazamiento, imagen);
            Storyboard.SetTargetProperty(desplazamiento, new PropertyPath("(" + propiedadCanvas + ")"));

            if (cicloInfinito)
            {
                desplazamiento.RepeatBehavior = RepeatBehavior.Forever;
            }
        }

        // La imagen indicada aparece suavemente
        private void AparecerImagen(Storyboard storyboard, Image imagen)
        {
            DoubleAnimation desaparicion = new DoubleAnimation();
            desaparicion.From = 0;
            desaparicion.To = 1;
            desaparicion.Duration = new Duration(TimeSpan.FromMilliseconds(800));

            storyboard.Children.Add(desaparicion);
            Storyboard.SetTarget(desaparicion, imagen);
            Storyboard.SetTargetProperty(desaparicion, new PropertyPath("Opacity", 1));

        }

        // La imagen desaparece aparece suavemente
        private void DesvanecerImagen(Storyboard storyboard, Image imagen)
        {
            DoubleAnimation desaparicion = new DoubleAnimation();
            desaparicion.From = 1;
            desaparicion.To = 0;
            desaparicion.Duration = new Duration(TimeSpan.FromMilliseconds(800));

            storyboard.Children.Add(desaparicion);
            Storyboard.SetTarget(desaparicion, imagen);
            Storyboard.SetTargetProperty(desaparicion, new PropertyPath("Opacity", 0));
        }

        // Inicia la animación de la vaca y el agua
        private void IniciarAnimacionConsecuenciaLluvia()
        {
            Storyboard sbConsecuenciasLluvia = FindResource("sbConsecuenciasLluvia") as Storyboard;
            sbConsecuenciasLluvia.Begin(this, true);
        }

        // Cambia la velocidad de la lluvia dependiendo 
        // del valor ingresado por el usuario
        private void CambiarVelocidadLluvia()
        {
            double velocidad = Convert.ToDouble(this.txtIndice.Text);
            velocidad = velocidad / 2;

            Storyboard sbLluvia = FindResource("sbLluvia") as Storyboard;
            sbLluvia.SetSpeedRatio(this, velocidad);
        }

        // Inicia la animación de la caída de la lluvia
        private void IniciarAnimacionLluvia()
        {
            Storyboard sbLluvia = FindResource("sbLluvia") as Storyboard;
            sbLluvia.Begin(this, true);
        }

        // Detiene la animación de la caída de la lluvia y esconde la nuve
        private void DetenerAnimacionLluvia()
        {
            Storyboard sbLluvia = FindResource("sbLluvia") as Storyboard;
            this.Nube.Visibility = Visibility.Hidden;
            sbLluvia.Stop(this);
        }
        
    }
}
