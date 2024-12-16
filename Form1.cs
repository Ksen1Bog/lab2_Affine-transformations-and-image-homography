using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV;
using Microsoft.VisualBasic;
using Emgu.CV.CvEnum;
using System.Drawing.Imaging;
using System.IO;

namespace dm1
{
    public partial class Form1 : Form
    {
        private PictureBox originalPictureBox;
        private PictureBox processedPictureBox;
        private Button scaleButton;
        private Button shiftButton;
        private Button rotateButton;
        private Button flipHorizontalButton;
        private Button flipVerticalButton;
        private List<PointF> selectedPoints = new List<PointF>();

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            LoadImage(); 
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "MainForm";
            this.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            AutoScaleMode = AutoScaleMode.Font;

            originalPictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(20, 20),
                Size = new Size(300, 300)
            };

            processedPictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(350, 20),
                Size = new Size(300, 300)
            };

            scaleButton = new Button
            {
                Text = "Масштабирование",
                Location = new Point(20, 350),
                Size = new Size(150, 30)
            };
            scaleButton.Click += ScaleButton_Click;

            shiftButton = new Button
            {
                Text = "Сдвиг",
                Location = new Point(190, 350),
                Size = new Size(150, 30)
            };
            shiftButton.Click += ShiftButton_Click;

            rotateButton = new Button
            {
                Text = "Поворот",
                Location = new Point(360, 350),
                Size = new Size(150, 30)
            };
            rotateButton.Click += RotateButton_Click;

            flipHorizontalButton = new Button
            {
                Text = "Отразить по горизонтали",
                Location = new Point(20, 390),
                Size = new Size(150, 30)
            };
            flipHorizontalButton.Click += FlipHorizontalButton_Click;

            flipVerticalButton = new Button
            {
                Text = "Отразить по вертикали",
                Location = new Point(190, 390),
                Size = new Size(150, 30)
            };
            flipVerticalButton.Click += FlipVerticalButton_Click;

            Controls.Add(originalPictureBox);
            Controls.Add(processedPictureBox);
            Controls.Add(scaleButton);
            Controls.Add(shiftButton);
            Controls.Add(rotateButton);
            Controls.Add(flipHorizontalButton);
            Controls.Add(flipVerticalButton);

            Button selectPointsButton = new Button
            {
                Text = "Выбрать точки",
                Location = new Point(360, 390),
                Size = new Size(150, 30)
            };
            selectPointsButton.Click += SelectPointsButton_Click;

            Button projectionButton = new Button
            {
                Text = "Проекция",
                Location = new Point(530, 390),
                Size = new Size(150, 30)
            };
            projectionButton.Click += ProjectionButton_Click;

            Controls.Add(selectPointsButton);
            Controls.Add(projectionButton);
        }

        private void FlipVerticalButton_Click(object sender, EventArgs e)
        {
            // Получение оригинального изображения
            Bitmap originalImage = (Bitmap)originalPictureBox.Image;

            // Создание нового Bitmap для отраженного изображения
            Bitmap flippedImage = new Bitmap(originalImage);

            // Отражение по вертикали
            flippedImage.RotateFlip(RotateFlipType.Rotate180FlipY);

            // Отображение обработанного изображения
            processedPictureBox.Image = flippedImage;
        }

        private void FlipHorizontalButton_Click(object sender, EventArgs e)
        {
            // Получение оригинального изображения
            Bitmap originalImage = (Bitmap)originalPictureBox.Image;

            // Создание нового Bitmap для отраженного изображения
            Bitmap flippedImage = new Bitmap(originalImage);

            // Отражение по горизонтали
            flippedImage.RotateFlip(RotateFlipType.Rotate180FlipX);

            // Отображение обработанного изображения
            processedPictureBox.Image = flippedImage;
        }

        private void RotateButton_Click(object sender, EventArgs e)
        {
            // Запрос параметров поворота у пользователя
            string inputAngle = Interaction.InputBox("Введите угол поворота (в градусах):");
            string inputPointX = Interaction.InputBox("Введите X-координату точки поворота:");
            string inputPointY = Interaction.InputBox("Введите Y-координату точки поворота:");

            double angle;
            double pointX, pointY;

            if (double.TryParse(inputAngle, out angle) && double.TryParse(inputPointX, out pointX) && double.TryParse(inputPointY, out pointY))
            {
                // Получение оригинального изображения
                Bitmap originalImage = (Bitmap)originalPictureBox.Image;

                // Создание нового Bitmap с размерами оригинального изображения
                Bitmap rotatedImage = new Bitmap(originalImage.Width, originalImage.Height);

                // Применение поворота с билинейной фильтрацией
                using (Graphics g = Graphics.FromImage(rotatedImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

                    // Поворот изображения
                    g.TranslateTransform((float)pointX, (float)pointY);
                    g.RotateTransform((float)angle);
                    g.TranslateTransform(-(float)pointX, -(float)pointY);
                    g.DrawImage(originalImage, new Point(0, 0));
                }

                // Отображение обработанного изображения
                processedPictureBox.Image = rotatedImage;
            }
            else
            {
                MessageBox.Show("Некорректные значения угла поворота и/или координат точки поворота.");
            }
        }

        private void ShiftButton_Click(object sender, EventArgs e)
        {
            // Запрос параметров сдвига у пользователя
            int shiftX, shiftY;
            string inputShiftX = Interaction.InputBox("Введите сдвиг по X:");
            string inputShiftY = Interaction.InputBox("Введите сдвиг по Y:");

            if (int.TryParse(inputShiftX, out shiftX) && int.TryParse(inputShiftY, out shiftY))
            {
                // Получение оригинального изображения
                Bitmap originalImage = (Bitmap)originalPictureBox.Image;

                // Создание нового Bitmap с размерами оригинального изображения
                Bitmap shiftedImage = new Bitmap(originalImage.Width, originalImage.Height);

                // Применение сдвига
                using (Graphics g = Graphics.FromImage(shiftedImage))
                {
                    g.DrawImage(originalImage, shiftX, shiftY);
                }

                // Отображение обработанного изображения
                processedPictureBox.Image = shiftedImage;
            }
            else
            {
                MessageBox.Show("Некорректные значения сдвига.");
            }
        }

        private void ScaleButton_Click(object sender, EventArgs e)
        {
            // Запрос параметров масштабирования у пользователя
            int newWidth, newHeight;
            string inputWidth = Interaction.InputBox("Введите новую ширину изображения:");
            string inputHeight = Interaction.InputBox("Введите новую высоту изображения:");

            if (int.TryParse(inputWidth, out newWidth) &&
                int.TryParse(inputHeight, out newHeight))
            {
                // Получение оригинального изображения
                Bitmap originalImage = (Bitmap)originalPictureBox.Image;

                // Создание нового Bitmap с заданными размерами
                Bitmap scaledImage = new Bitmap(newWidth, newHeight);

                // Использование Graphics для масштабирования изображения с билинейной фильтрацией
                using (Graphics g = Graphics.FromImage(scaledImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                    g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                }

                // Отображение обработанного изображения
                processedPictureBox.Image = scaledImage;
            }
            else
            {
                MessageBox.Show("Некорректные значения ширины и/или высоты.");
            }
        }

        private void LoadImage()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp"; // Фильтр для файлов изображений

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Загрузка изображения из выбранного файла
                        Image img = Image.FromFile(openFileDialog.FileName);
                        originalPictureBox.Image = img;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message);
                    }
                }
            }
        }

        private void SelectPointsButton_Click(object sender, EventArgs e)
        {
            // Сбрасываем выбранные точки и активируем обработчик события щелчка мыши
            selectedPoints.Clear();
            originalPictureBox.MouseClick += OriginalPictureBox_MouseClick;
        }
        private void ProjectionButton_Click(object sender, EventArgs e)
        {
            // Проверяем, что выбрано ровно четыре точки
            if (selectedPoints.Count != 4)
            {
                MessageBox.Show("Для проекции необходимо выбрать четыре точки на исходном изображении.");
                return;
            }

            // Создаем массив точек из списка выбранных точек
            PointF[] sourcePoints = selectedPoints.ToArray();

            // Задаем четыре точки плоскости проекции
            PointF[] destPoints = new PointF[]
            {
            new PointF(0, 0),
            new PointF(0, originalPictureBox.Image.Height - 1),
            new PointF(originalPictureBox.Image.Width - 1, originalPictureBox.Image.Height - 1),
            new PointF(originalPictureBox.Image.Width - 1, 0)
            };

            // Получаем матрицу гомографической проекции
            var homographyMatrix = CvInvoke.GetPerspectiveTransform(sourcePoints, destPoints);

            // Преобразуем Bitmap в массив байтов
            using (MemoryStream ms = new MemoryStream())
            {
                ((Bitmap)originalPictureBox.Image).Save(ms, ImageFormat.Bmp);
                byte[] imageBytes = ms.ToArray();

                // Проектируем изображение
                using (Mat sourceMat = new Mat())
                {
                    // Декодируем изображение из массива байтов
                    CvInvoke.Imdecode(imageBytes, ImreadModes.AnyColor, sourceMat);

                    using (Mat destMat = new Mat())
                    {
                        CvInvoke.WarpPerspective(sourceMat, destMat, homographyMatrix, sourceMat.Size);

                        // Преобразуем результат в Bitmap и отображаем на PictureBox
                        using (Image<Bgr, byte> destImage = destMat.ToImage<Bgr, byte>())
                        {
                            Bitmap destBitmap = destImage.ToBitmap();
                            processedPictureBox.Image = destBitmap;
                        }
                    }
                }
            }
        }

        private void OriginalPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            // Получение размеров PictureBox и изображения
            int pictureBoxWidth = originalPictureBox.Width;
            int pictureBoxHeight = originalPictureBox.Height;
            int imageWidth = originalPictureBox.Image.Width;
            int imageHeight = originalPictureBox.Image.Height;

            // Расчет масштаба изображения по осям X и Y
            float scaleX = (float)imageWidth / pictureBoxWidth;
            float scaleY = (float)imageHeight / pictureBoxHeight;

            // Пересчет координат клика мыши с учетом масштабирования
            float imageX = e.X * scaleX;
            float imageY = e.Y * scaleY;

            // Создание PointF с пересчитанными координатами
            PointF clickedPoint = new PointF(imageX, imageY);

            // Добавление точки в список выбранных точек
            selectedPoints.Add(clickedPoint);

            // Отображение закрашенных кругов на исходном изображении
            using (Graphics g = Graphics.FromImage(originalPictureBox.Image))
            {
                foreach (PointF point in selectedPoints)
                {
                    int radius = 8; 
                    using (SolidBrush brush = new SolidBrush(Color.Red)) 
                    {
                        g.FillEllipse(brush, point.X - radius, point.Y - radius, 2 * radius, 2 * radius);
                    }
                }
            }

            // Обновление PictureBox, чтобы отобразить изменения
            originalPictureBox.Invalidate();

            // Если выбраны четыре точки, блокируем обработчик события и вызываем процесс проекции
            if (selectedPoints.Count == 4)
            {
                originalPictureBox.MouseClick -= OriginalPictureBox_MouseClick;
                MessageBox.Show("Выбрано четыре точки. Нажмите кнопку 'Проекция', чтобы продолжить.");
            }
        }
    }
}

