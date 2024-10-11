// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System;
using FileHelpers;
using System.IO;
using static TwowayFunction.DataBasicHandle;
using System.Runtime.Intrinsics.Arm;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualBasic;
using System.Security.Cryptography;



namespace TwowayFunction
{


    class Product
    {
        public string PathDataHPQ = "C:/Users/Admin/Desktop/F5 Pack/Dữ liệu nội suy HPQ.csv";
        public string PathDataQETA = "C:/Users/Admin/Desktop/F5 Pack/Dữ liệu nội suy QETA.csv";
        public string PathDataQHL = "C:/Users/Admin/Desktop/F5 Pack/Dữ liệu nội suy QHL.csv";
        public string line;

        public double S = 0.0;


        public void findValueBoardQHL()
        {
            double[] cp1 = new double[2];
            double[] cp2 = new double[2];
            double v0 = 0.00004;
            bool firstline = true;

            try
            {
                using (StreamReader reader = new StreamReader(PathDataQHL))
                {
                    // Read and display the first line (header)
                    line = reader.ReadLine();
                    Console.WriteLine(line);

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!firstline)
                        {

                            cp2[0] = cp1[0]; // q q
                            cp2[1] = cp1[1]; // h v
                        }
                        string[] values = line.Split(',');

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = values[i].Trim('"');
                            if (double.TryParse(values[i], out double value))
                            {
                                if (i == 0)
                                {
                                    cp1[0] = value;
                                }
                                else if (i == 1)
                                {
                                    cp1[1] = value;
                                }
                                //Console.WriteLine($"{value:F5}");
                            }
                            else
                            {
                                Console.WriteLine($"Invalid number: {values[i]:F5}");
                            }
                        }

                        if (!firstline)
                        {
                            // kiem tra thoe dieu kien 
                            if (v0 > cp2[1] && v0 > cp1[1])
                            {
                                continue;
                            }
                            else if (v0 > cp2[1] && v0 < cp1[1])
                            {
                                S = cp2[0] + ((v0 - cp2[1]) * (cp1[0] - cp2[0])) / (cp1[1] - cp2[1]);
                                Console.WriteLine(S);

                            }
                            else if (v0 == cp2[1])
                            {
                                S = cp2[0];
                                Console.WriteLine(S);

                            }
                        }
                        firstline = false;

                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public void note()
        {
            /*
          using(StreamReader reader = new StreamReader(PathDataHPQ)) 
          using(StreamWriter writer = new StreamWriter(outpath))
          {
              lines = reader.ReadLine(); 
              while((lines = reader.ReadLine()) != null)
              {
                  string[] arr = lines.Split(',');

                  if (arr[0] == "\"1\"")
                  {
                      writer.WriteLine($"{arr[1]},{arr[2]},{arr[3]}");
                  }   

              }

          }
          */

            /*
         List<DataHandle.Data3D> DataPoint =datahandle.LoadData3D(outpath);
         double result  = datahandle.Calculate3D(DataPoint, 24.1 , 1.2);
         Console.WriteLine(result);
         */

            //double Po = datahandle.CalQplant(Pi, ho);




        }
        public static void themdulieu3(string input, List<DataHandle.Data2D> data1)
        {


            double a;

            List<string> ketqua3 = new List<string>();
            List<double> s = new List<double>();
            foreach (var item in data1)
            {
                a = DataHandle.CalculateQHL_Q(data1, item.Z);
                if (a != null)
                {
                    s.Add(a);
                }
            }

            using (StreamWriter rd = new StreamWriter(input))
            {
                string line;
                int i = 0;
                foreach (var item in data1)
                {

                    // Ghép 4 trường thành một dòng mới
                    string newLine = $"{item.Z}; {item.Q};{s.ElementAt(i)}";
                    ketqua3.Add(newLine);
                    i++;
                }

                rd.WriteLine("Z, Q , Test");
                foreach (var item in ketqua3)
                {
                    rd.WriteLine(item);
                }

            }

        }
        public static void themdulieu2(string input, List<DataHandle.DataQETA> data2)
        {
            double a;

            List<string> ketqua2 = new List<string>();
            List<double> ETA = new List<double>();
            foreach (var item in data2)
            {
                a = DataHandle.CalculateQETA_Eta(data2, item.QQMax);
                if (a != null)
                {
                    ETA.Add(a);
                }
            }

            using (StreamWriter rd = new StreamWriter(input))
            {
                string line;
                int i = 0;
                foreach (var item in data2)
                {

                    // Ghép 4 trường thành một dòng mới
                    string newLine = $"{item.QQMax}; {item.Eta};{ETA.ElementAt(i)}";
                    ketqua2.Add(newLine);
                    i++;
                }

                rd.WriteLine("QQmax, ETA , Test");
                foreach (var item in ketqua2)
                {
                    rd.WriteLine(item);
                }

            }

        }
        public static void themdulieuTestZV(string input, List<DataHandle.Data2D> data, List<double> Qtest)
        {


            List<string> strings = new List<string>();
            using (StreamWriter rd = new StreamWriter(input))
            {
                string line;
                int i = 0;
                foreach (var item in data)
                {

                    // Ghép 4 trường thành một dòng mới
                    string newLine = $"; {item.Z};".PadRight(15, ' ') + $"{item.Q} - {Qtest.ElementAt(i):F4}";
                    strings.Add(newLine);
                    i++;
                }

                rd.WriteLine("z,v - test");
                foreach (var item in strings)
                {
                    rd.WriteLine(item);
                }

            }

        }


        public static void themdulieu(string input, List<DataHandle.Data3D> data , List<double> Qtest)
        {
            

            List<string> strings = new List<string>();
            using (StreamWriter rd = new StreamWriter(input))
            {
                string line;
                int i = 0;
                foreach (var item in data)
                {

                    // Ghép 4 trường thành một dòng mới
                    string newLine = $"{item.H}; {item.P};".PadRight(15,' ')  +  $"{item.Q}-{Qtest.ElementAt(i):F4}";
                    strings.Add(newLine);
                    i++;
                }

                rd.WriteLine("h,p,q,A");
                foreach (var item in strings)
                {
                    rd.WriteLine(item);
                }

            }

        }

        static void Main(string[] args)
        {
            //file data su dung
            string input = "C:/Users/Admin/Documents/Zalo Received Files/data.txt";
            string outpath = "C:/Users/Admin/Desktop/F5 Pack/nội suy HPQ.csv";
            string outpath2 = "C:/Users/Admin/Desktop/F5 Pack/nội suy HPQ2.csv";
            string Song_HPQ = "C:/Users/Admin/Documents/Zalo Received Files/SongChay3/SongChay3/SongChay3_2HPQ_TestCase.csv";

            // file luu du lieu test
            string ds2 = "C:/Users/Admin/Documents/Zalo Received Files/dataQETA.txt";
            string ds3 = "C:/Users/Admin/Documents/Zalo Received Files/dataQHL.txt";
            string input2 = "C:/Users/Admin/Documents/Zalo Received Files/dataQETA.txt";
            string input3 = "C:/Users/Admin/Documents/Zalo Received Files/dataQHL.txt";

            // file data goc 
            string PathDataHPQ = "C:/Users/Admin/Desktop/F5 Pack/Dữ liệu nội suy HPQ.csv";
            string PathDataQETA = "C:/Users/Admin/Desktop/F5 Pack/Dữ liệu nội suy QETA.csv";
            string PathDataQHL = "C:/Users/Admin/Desktop/F5 Pack/Dữ liệu nội suy QHL.csv";
            string PathdataZV = "C:/Users/Admin/Documents/Zalo Received Files/SongChay3/SongChay3/SongChay3_ZV.csv";

            // file test
            string PathTestZV = "C:/Users/Admin/Documents/Zalo Received Files/SongChay3/SongChay3/SongChay3_ZV_TestCase.csv";

            DataHandle datahandle = new DataHandle();
            List<DataHandle.Data2D> data1 = datahandle.LoadData2D(PathDataQHL);
            List<DataHandle.DataQETA> data2 = datahandle.LoadQETA(PathDataQETA);
            List<DataHandle.Data3D> data = datahandle.LoadData3D(outpath2 ); // HPQ
            List<DataHandle.Data2D> daya = datahandle.LoadDataZV(PathdataZV);

            List<DataHandle.Data3D> dataSong_HPQ = datahandle.LoadData3D(Song_HPQ);
            List<DataHandle.Data2D> dataTestZV = datahandle.LoadTestZV(PathTestZV);

            double a  = 0;

            Console.WriteLine($"ket qua cua a : {a}");
            List<double > Qtest = new List<double>();
            
            //// Chay luu du lieu vao file 
            /*
            foreach (var it in dataTestZV)
            {

                a = datahandle.CalculateZV_V(daya, it.Z);
                if (a != null)
                {
                    Qtest.Add(a);
                }

            }
            int x = 0;
            foreach (var it in Qtest)
            {
                
                Console.WriteLine($" {dataTestZV.ElementAt(x).Q}  - {it} ");
                x++;
            }
            */

            // Ham chay luu du lieu 
            //themdulieu(input, dataSong_HPQ, Qtest);
            //themdulieuTestZV(PathTestZV, dataTestZV, Qtest);
            //themdulieu2(input2,data2);
            //themdulieu3(input3,data1);



            double z_r = 400.059;
            double z_t = 356.66;
            double hl = 0;
            double[] p = { 13.6  };
            double Q_MaxUnit = 38.370;

            double x = datahandle.CalculateZV_V(daya, 397.781);
            Console.WriteLine($"{x}");
            
            double ho = datahandle.CalculateHPQ_HuuIch(data, z_r, z_t, hl);
            double P0 = datahandle.CalQplant(p, ho);

            double Qo = datahandle.Calculate_QoLow(data, ho, P0);
            
            //double Qo = CalculateQovaQtop(data3Ds, data2Ds, P0, Ho, false);  // qoLow
            double Q_top = datahandle.CalculateQtop(data, data1, P0, ho, true); // QoTop
            Console.WriteLine($"Qo : {Qo}  Qotop : {Q_top}");
            double Q = datahandle.BinarySearch(data1, data, data2, p, z_t, z_r, hl, false,2, 2, Q_MaxUnit);
            if(Q <= 0)
            {
                Console.WriteLine(" Ket qua khong tim duoc : -1");
            }
            else {

                Console.WriteLine("ket qua q tim duoc la : {0:F5}", Q);
            }
            GeneratedBroad(data, ho, p,Q);



        }


        public static void GeneratedBroad(List<DataHandle.Data3D> data , double ho, double[] p, double Q)
        {

            Console.WriteLine();


            //  khoi tao du lieu bang hashset
            HashSet<double> uniqueP = new HashSet<double>();
            HashSet<double> uniqueH = new HashSet<double>();

            foreach (var point in data)
            {
                uniqueP.Add(point.P);
                uniqueH.Add(point.H);
            }
            List<double> uniquePlist = uniqueP.ToList();
            List<double> uniqueHlist = uniqueH.ToList();

            bool x = uniquePlist.Contains(p[0]);
            bool y = uniquePlist.Contains(ho);

            int width = 7;
            string noRS = "|-".PadRight(width, ' ');
            string pama = null;

            if (!x)
            {
                for (int i = 0; i < uniquePlist.Count ; i++)
                {
                    if (uniquePlist[i] < p[0] && uniquePlist[i + 1] > p[0])
                    {
                        uniquePlist.Insert(i + 1, p[0]);
                        break;
                    }
                }

            }
            if (!y)
            {
                for (int i = 0; i < uniqueHlist.Count ; i++)
                {
                    if (uniqueHlist[i] < ho && uniqueHlist[i + 1] > ho)
                    {
                        uniqueHlist.Insert(i + 1, ho);
                        break;
                    }
                }
            }

            int lengthP = uniquePlist.Count;
            int lengthH = uniqueHlist.Count;


            // in tieu de P 
            Console.Write(new string(' ', width));
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var point in uniquePlist)
            {
               
                    if (point != p[0])
                    {
                        Console.Write(point.ToString().PadRight(width));
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(point.ToString().PadRight(width));
                        Console.ForegroundColor = ConsoleColor.Green;

                    }
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(new string('-', 50));

            // in cot tieu de H 
            for (int i = 0; i < lengthH; i++)
            {
                // in gia tri tieu de o cot H dau tien 
                double epsilon = 1e-6; // Bạn có thể điều chỉnh giá trị này tùy theo yêu cầu độ chính xác
                if (Math.Abs(uniqueHlist.ElementAt(i) - ho) < epsilon)
                {

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    pama = $"|{uniqueHlist.ElementAt(i)}".PadRight(width);
                    Console.Write(pama);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {

                    Console.ForegroundColor = ConsoleColor.Green;
                    pama = $"|{uniqueHlist.ElementAt(i)}".PadRight(width);
                    Console.Write(pama);
                    Console.ForegroundColor = ConsoleColor.White;

                }

                for (int j = 0; j < lengthP; j++)
                {
                    var point = data.Find(x => x.H == uniqueHlist.ElementAt(i) && x.P == uniquePlist.ElementAt(j));
                    if (point != null)
                    {
                        if (Math.Abs(point.H - ho) < epsilon || point.P == p[0])
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            pama = $"|{point.Q}".PadRight(width);
                            Console.Write(pama);
                            Console.ForegroundColor = ConsoleColor.White;

                        }
                        else
                        {
                            pama = $"|{point.Q}".PadRight(width);
                            Console.Write(pama);
                        }

                    }
                    else
                    {
                        if (uniquePlist.ElementAt(j) == p[0] || Math.Abs( uniqueHlist.ElementAt(i) - ho) < epsilon)
                        {
                            if (uniquePlist.ElementAt(j) == p[0] && Math.Abs(uniqueHlist.ElementAt(i) - ho) < epsilon)
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                pama = $"|{Q:F2}".PadRight(width);
                                Console.Write(pama);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else   
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.Write(noRS);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                        else
                        {
                            Console.Write(noRS);
                        }
                      
                    }

                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.White;

        }

        public static void SuaTCsongchay()
        {
            // Đường dẫn tệp (hãy thay thế đường dẫn này bằng đường dẫn đến tệp của bạn)
            string filePath = "C:/Users/Admin/Documents/Zalo Received Files/HPQ_songchay3_testcase.csv";

            // Đọc tất cả các dòng trong tệp
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)
            {
                Console.WriteLine("File không chứa đủ dữ liệu.");
                return;
            }

            // Lấy tiêu đề từ dòng đầu tiên và chia các phần tử bằng dấu phân cách `;`
            string[] headers = lines[0].Split(';');

            // Lấy các giá trị từ các dòng tiếp theo (bắt đầu từ dòng thứ 2)
            string[] firstColumnValues = lines.Skip(1).Select(line => line.Split(';')[0]).ToArray();

            // Định nghĩa chiều rộng của cột để in đẹp hơn
            int columnWidth = 15;

            // In tiêu đề của bảng (cột dọc đầu tiên là phần tử đầu tiên của dòng đầu tiên)
            Console.Write("".PadRight(columnWidth)); // Ô trống cho cột góc
            foreach (string value in firstColumnValues)
            {
                Console.Write(value.PadRight(columnWidth));
            }
            Console.WriteLine();

            // In các giá trị trong dòng đầu tiên (hàng ngang)
            for (int i = 1; i < headers.Length; i++)
            {
                Console.Write(headers[i].PadRight(columnWidth));

                // In các giá trị tương ứng từ các dòng tiếp theo
                for (int j = 0; j < firstColumnValues.Length; j++)
                {
                    string[] values = lines[j + 1].Split(';');
                    if (i < values.Length)
                    {
                        Console.Write(values[i].PadRight(columnWidth));
                    }
                    else
                    {
                        Console.Write("".PadRight(columnWidth)); // Ô trống nếu không có giá trị
                    }
                }
                Console.WriteLine();
            }
        

    }
    public static void run ( double z_r, double z_t, double hl, double[] pi , bool x , string a, string b, string c)
        {

            DataHandle datahandle = new DataHandle();

            List<DataHandle.Data2D> data1 = datahandle.LoadData2D(c);
            List<DataHandle.DataQETA> data2 = datahandle.LoadQETA(b);
            List<DataHandle.Data3D> data = datahandle.LoadData3D(a); // HPQ

            //   truong hop khong co thiet bij do luu luong qua cac to may 
            // nha may thie ke voi cac to may cung loai va cung cong suat P  -> 1 bang tra HPQ tra luu luong 

            
            double ho = datahandle.CalculateHPQ_HuuIch(data, z_r, z_t, hl);
            double Q = datahandle.BinarySearch(data1, data, data2, pi, z_t, z_r, hl, x, 2,2, 38.370);
            Console.WriteLine("ket qua q tim duoc la : {0:F5}", Q);

        }

    }
}

