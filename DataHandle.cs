using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static TwowayFunction.DataBasicHandle;
using static TwowayFunction.DataHandle;

namespace TwowayFunction
{
    internal class DataHandle
    {
        //   ______________ ATTRIBUTED DATA ____________________ 
        public class Data3D
        {
            public double H { get; set; }
            public double P { get; set; }
            public double Q { get; set; }

        }

        public class DataQETA
        {
            public double QQMax { get; set; }
            public double Eta { get; set; }
        }

        public class Data2D
        {
            public double Z { get; set; }
            public double Q { get; set; }
        }

        //  _____________________                               LOAD DATA _________________

        public List<DataQETA> LoadQETA(string filename) {
            
            List<DataQETA> data = new List<DataQETA>();
            using (StreamReader rd = new StreamReader(filename))
            {
                string line;
                line = rd.ReadLine();
                while ((line = rd.ReadLine()) != null)
                {

                    string[] values = line.Split(',');
                    if(values.Length == 2 
                        && double.TryParse(values[0].Trim('"'), out double qqmax) 
                        && double.TryParse(values[1].Trim('"'),out double eta))
                    {
                        data.Add(new DataQETA { QQMax = qqmax, Eta = eta });
                    }
                   

                }
            }
            return data;
        }

        public List<Data2D> LoadData2D(string filePath)
        {
            List<Data2D> dataPoints = new List<Data2D>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');
                    if (values.Length == 2 &&
                        double.TryParse(values[0].Trim('"'), out double q) &&
                        double.TryParse(values[1].Trim('"'), out double z))
                    {
                        dataPoints.Add(new Data2D { Q = q, Z = z });
                    }
                }
            }

            return dataPoints;
        }
        
        public List<Data2D> LoadDataZV(string filePath)
        {
            List<Data2D> dataPoints = new List<Data2D>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');
                    if (values.Length == 2 &&
                        double.TryParse(values[0].Trim('"'), out double z) &&
                        double.TryParse(values[1].Trim('"'), out double q))
                    {
                        dataPoints.Add(new Data2D {  Z = z , Q = q });
                    }
                }
            }

            return dataPoints;
        }

        public List<Data3D> LoadData3D(string filename) 
        {
            List<Data3D> dataPoints = new List<Data3D>();
            using(StreamReader rd = new StreamReader(filename))
            {
                string line;
                while ((line = rd.ReadLine()) != null)
                {

                    string[] vlues = line.Split(',');
                    if(vlues.Length == 3 && double.TryParse(vlues[0].Trim('"'),out double h) && double.TryParse(vlues[1].Trim('"'),out double p) && double.TryParse(vlues[2].Trim('"'), out double q)) 
                    {
                        dataPoints.Add(new Data3D { H = h, P = p, Q = q });
                    }
                }
            }

            return dataPoints;
        }

        public List<Data2D> LoadTestZV(string filename)
        {
            List<Data2D> dataPoints = new List<Data2D>();
            using (StreamReader rd = new StreamReader(filename))
            {
                string line;
                line = rd.ReadLine();
                while ((line = rd.ReadLine()) != null)
                {

                    string[] vlues = line.Split(',');
                    if (vlues.Length == 2 && double.TryParse(vlues[0], out double z) && double.TryParse(vlues[1], out double q))
                    {
                        dataPoints.Add(new Data2D { Z = z, Q = q });
                    }
                }
            }

            return dataPoints;
        }

        // _________________________                            FUNCTION _________________ ALpha



        // -------------------------                            Q - ETA  -----------------

        public static double CalculateQETA_Eta(List<DataQETA> DataQETA_ETA,  double Tile)// q mid unit / q max unit
        {
            // Tìm giá trị gần nhất

            var ETA = DataQETA_ETA.OrderBy(dp => Math.Abs(dp.QQMax - Tile)).FirstOrDefault();

            // Kiểm tra nếu không tìm thấy giá trị phù hợp hoặc nếu ETA là null
            if (ETA == null || ETA.Eta == null)
            {
                throw new Exception("Không có giá trị ETA phù hợp.");
            }
            return ETA.Eta;
        }



        // -------------------------                            Q - H Loss ---------------
        public double CalculateQHL_HLoss(List<Data2D> data2D_QHL, double targetQ)
        {
            Data2D data2d = data2D_QHL.Find(dp => dp.Q == targetQ);
            if (data2d != null)
            {
                return data2d.Z;
            }

            Data2D Ai = data2D_QHL.LastOrDefault(dp => dp.Q < targetQ);
            Data2D Ai1 = data2D_QHL.FirstOrDefault(dp => dp.Q > targetQ);

           
            double Zo = Ai.Z +  ( (targetQ - Ai.Q) * (Ai1.Z - Ai.Z) ) / ( Ai1.Q - Ai.Q )  ;
 
            return Zo;


            throw new Exception("Không tìm thấy giá trị phù hợp trong dữ liệu.");

        }

        public static double CalculateQHL_Q(List<Data2D> data2D_QHL, double targetH)
        {
            double epsilon = 0.0000001;
            Data2D data2d = data2D_QHL.Find(dp => Math.Abs(dp.Z - targetH) < epsilon);
            if (data2d != null)
            {
                return data2d.Q;
            }

            Data2D Ai = data2D_QHL.LastOrDefault(dp => dp.Z < targetH);
            Data2D Ai1 = data2D_QHL.FirstOrDefault(dp => dp.Z > targetH);

            double Vo = Ai.Q + ((targetH - Ai.Z) * (Ai1.Q - Ai.Q)) / (Ai1.Z - Ai.Z);
            return Vo;


            throw new Exception("Không tìm thấy giá trị phù hợp trong dữ liệu.");

        }

        public double CalculateZV_V(List<Data2D> data2D_QHL, double Z)
        {
            double epsilon = 0.0000001;
            Data2D PointZ0 = data2D_QHL.Find(dp => Math.Abs(dp.Z - Z) < epsilon);
            if(PointZ0 != null)
            {
                return PointZ0.Q;
            }

            Data2D Zi = data2D_QHL.Where(dp => dp.Z < Z).OrderByDescending(dp => dp.Z).FirstOrDefault();
            Data2D Zi1 = data2D_QHL.Where(dp => dp.Z > Z).OrderBy(dp => dp.Z).FirstOrDefault();

            double Vo = Zi.Q + ((Z - Zi.Z)*(Zi1.Q - Zi.Q)) / (Zi1.Z - Zi.Z);
            
            return Vo;

            throw new Exception("Không tìm thấy giá trị phù hợp trong dữ liệu.");
        }



        // ------------------------------                      Q - H - L ----------------------
        public double Calculate_QoLow(List<Data3D> dataPoints, double targetH, double targetP)
        {
            double epsilon = 0.000001;
            Data3D data3D = dataPoints.Find(dp => Math.Abs(dp.H - targetH) < epsilon && Math.Abs(dp.P - targetP) < epsilon);
            if(data3D != null)
            {
                return data3D.Q;

            }

            Data3D matchH = dataPoints.FirstOrDefault(dp => Math.Abs(dp.H - targetH) < epsilon && Math.Abs(dp.P - targetP) >= epsilon);
            Data3D matchP = dataPoints.FirstOrDefault(dp => Math.Abs(dp.H - targetH) >= epsilon && Math.Abs(dp.P - targetP) < epsilon);

            if (matchH != null && matchP == null)
            {
                // Xử lý nội suy cho trường hợp chỉ trùng targetH
                Data3D QojH = dataPoints.LastOrDefault(dp => Math.Abs(dp.H - targetH) < epsilon && dp.P < targetP);
                Data3D Qoj1H = dataPoints.FirstOrDefault(dp => Math.Abs(dp.H - targetH) < epsilon && dp.P > targetP);

                if (QojH != null && Qoj1H != null)
                {
                    double Qo0H = QojH.Q + ((targetP - QojH.P) * (Qoj1H.Q - QojH.Q)) / (Qoj1H.P - QojH.P);
                    return Qo0H;
                }
            }
            else if (matchP != null && matchH == null)
            {
                // Xử lý nội suy cho trường hợp chỉ trùng targetP
                Data3D QijP = dataPoints.LastOrDefault(dp => dp.H < targetH && dp.P < targetP);
                Data3D Qi1j1P = dataPoints.FirstOrDefault(dp => dp.H > targetH && dp.P > targetP);
                Data3D Qi1jP = dataPoints.FirstOrDefault(dp => dp.H > targetH && dp.P < targetP);
                Data3D Qij1P = dataPoints.LastOrDefault(dp => dp.H < targetH && dp.P > targetP);

                if (QijP != null && Qi1j1P != null)
                {

                    double QojP = QijP.Q + ((targetH - QijP.H) * (Qi1jP.Q - QijP.Q)) / (Qi1jP.H - QijP.H);
                    double Qoj1P = Qij1P.Q + ((targetH - QijP.H) * (Qi1j1P.Q - Qij1P.Q)) / (Qi1jP.H - QijP.H);
                    double Qo0P = QojP + ((targetP - QijP.P) * (Qij1P.Q - QojP)) / (Qij1P.P - QijP.P);
                    return Qo0P;
                }
            }

            //  // Tìm các giá trị gần targetH và targetP nhất
            Data3D Qij = dataPoints
                .Where(dp => dp.H <= targetH && dp.P <= targetP)
                .OrderByDescending(dp => dp.H)
                .ThenByDescending(dp => dp.P)
                .FirstOrDefault();

            Data3D Qi1j = dataPoints
                .Where(dp => dp.H > targetH && dp.P <= targetP)
                .OrderBy(dp => dp.H)
                .ThenByDescending(dp => dp.P)
                .FirstOrDefault();

            Data3D Qij1 = dataPoints
                .Where(dp => dp.H <= targetH && dp.P > targetP)
                .OrderByDescending(dp => dp.H)
                .ThenBy(dp => dp.P)
                .FirstOrDefault();

            Data3D Qi1j1 = dataPoints
                .Where(dp => dp.H > targetH && dp.P > targetP)
                .OrderBy(dp => dp.H)
                .ThenBy(dp => dp.P)
                .FirstOrDefault();

            if ( Qij != null && Qi1j != null && Qij1 != null && Qi1j1 != null)
            {
                double Qoj = Qij.Q + ((targetH - Qij.H) * (Qi1j.Q - Qij.Q)) / (Qi1j.H - Qij.H);
                double Qoj1 = Qij1.Q + ((targetH - Qij.H) * (Qi1j1.Q - Qij1.Q)) / (Qi1j.H - Qij.H);
                double Qo0 = Qoj + ((targetP - Qij.P) * (Qoj1 - Qoj)) / (Qij1.P - Qij.P);
                return Qo0;

            }else
            {
                return 0;
            }

        }

        public double CalculateQtop(List<Data3D> DataPoints, List<Data2D> DataPoint2D, double P0, double H0, bool dk)
        {
            var ListofP = DataPoints.Select(dp => dp.P).Distinct().OrderBy(p => p).ToList();

            var exactP0 = DataPoints.FirstOrDefault(dp => Math.Abs(dp.P - P0) < 0.00001);


            if (exactP0 != null) // truong hop P0 co trong du lieu 
            {
                var PointQolow = Calculate_QoLow(DataPoints, H0, P0);

                double QoTop = 0;
                double Pi = P0 - 0.5;
                double Pi1 = P0 + 0.5;

                List<Data3D> ListPo = DataPoints.Where(x => x.P == P0).OrderBy(x => x.H).ToList();

                List<Data3D> ListPi = DataPoints.Where(x => x.P == Pi).OrderBy(x => x.H).ToList();

                List<Data3D> ListPi1 = DataPoints.Where(x => x.P == (Pi1)).OrderBy(x => x.H).ToList();

                /// co gia tri qo top  truoc qolow 
                var Qotop = ListPo.FirstOrDefault(dp => dp.Q != null && dp.Q > PointQolow && dp.H < H0);

                if (QoTop != default)
                {
                    return QoTop;
                }
                else
                {
                    var listofH = DataPoints.Select(x => x.H).Distinct().OrderBy(h => h).ToList();

                    foreach (var p in listofH)
                    {
                        double epsilon = 0.000001;
                        int indexQItop = ListPi.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);

                        int indexQI1Top = ListPi1.FindIndex(x => Math.Abs(x.H - (p )) < epsilon);


                        if (indexQItop >= 0 && indexQI1Top >= 0)
                        {
                            var Point_QItop = ListPi.ElementAt(indexQItop);

                            var Point_QI1Top = ListPi1.ElementAt(indexQI1Top);


                            if (Point_QItop != null && Point_QItop != null)
                            {

                                QoTop = Point_QItop.Q + ((P0 - Point_QItop.P) * (Point_QI1Top.Q - Point_QItop.Q)) / (Point_QI1Top.P - Point_QItop.P);


                                if (QoTop != 0)
                                {

                                    Console.WriteLine("tinh toan thanh cong");
                                    return QoTop;

                                }

                            }
                        }
                    }



                }


            }
            else if (exactP0 == null)
            {  // truong hop khong co po trong du lieu 

                var exactH0 = DataPoints.FirstOrDefault(dp => Math.Abs(dp.H - H0) < 0.00001);

                if (exactH0 != default)
                {

                    double QoTop = 0;
                    double Pi = ListofP.LastOrDefault(dp => dp < P0);
                    double Pi1 = ListofP.FirstOrDefault(dp => dp > P0);

                    List<Data3D> ListPi = DataPoints.Where(x => Math.Abs(x.P - Pi) < 0.0001).OrderBy(x => x.H).ToList();
                    List<Data3D> ListPi1 = DataPoints.Where(x => Math.Abs(x.P - Pi1) < 0.0001).OrderBy(x => x.H).ToList();
                   
                    var listofH = DataPoints.Select(x => x.H).Distinct().OrderBy(h => h).ToList();

                    foreach (var p in listofH)
                    {
                        double epsilon = 0.000001;
                        int indexQItop = ListPi.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);

                        int indexQI1Top = ListPi1.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);


                        if (indexQItop >= 0 && indexQI1Top >= 0)
                        {
                            var Point_QItop = ListPi.ElementAt(indexQItop);

                            var Point_QI1Top = ListPi1.ElementAt(indexQI1Top);


                            if (Point_QItop != null && Point_QI1Top != null)
                            {

                                QoTop = Point_QItop.Q + ((P0 - Point_QItop.P) * (Point_QI1Top.Q - Point_QItop.Q)) / (Point_QI1Top.P - Point_QItop.P);


                                if (QoTop != 0)
                                {

                                    Console.WriteLine("tinh toan thanh cong");
                                    return QoTop;

                                }

                            }
                        }
                    }

                }
                else
                {

                    double QoTop = 0;
                    double Pi = ListofP.LastOrDefault(dp => dp < P0);
                    double Pi1 = ListofP.FirstOrDefault(dp => dp > P0);

                    List<Data3D> ListPi = DataPoints.Where(x => Math.Abs(x.P - Pi) < 0.0001).OrderBy(x => x.H).ToList();
                    List<Data3D> ListPi1 = DataPoints.Where(x => Math.Abs(x.P - Pi1) < 0.0001).OrderBy(x => x.H).ToList();

                    var listofH = DataPoints.Select(x => x.H).Distinct().OrderBy(h => h).ToList();

                    foreach (var p in listofH)
                    {
                        double epsilon = 0.000001;
                        int indexQItop = ListPi.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);

                        int indexQI1Top = ListPi1.FindIndex(x => Math.Abs(x.H - (p )) < epsilon);


                        if (indexQItop >= 0 && indexQI1Top >= 0)
                        {
                            var Point_QItop = ListPi.ElementAt(indexQItop);

                            var Point_QI1Top = ListPi1.ElementAt(indexQI1Top);


                            if (Point_QItop != null && Point_QI1Top != null)
                            {

                                QoTop = Point_QItop.Q + ((P0 - Point_QItop.P) * (Point_QI1Top.Q - Point_QItop.Q)) / (Point_QI1Top.P - Point_QItop.P);


                                if (QoTop != 0)
                                {

                                    Console.WriteLine("tinh toan thanh cong");
                                    return QoTop;

                                }

                            }
                        }
                    }
                }


            }

            throw new Exception(" khong ton tai");
        }

        public double CalculateHPQ_HuuIch(List<Data3D> DataPoint, double Z_res, double Z_tail, double Headloss )
        {
            double H_max = DataPoint.Max(dp => dp.H);
            double H_o;
            double h_1 = DataPoint.First().H;
            if ( Headloss == 0 )
            {
                H_o = Z_res - Z_tail;

                if(H_o >= H_max)
                {
                    H_o = H_max;
                    return H_o;
                }
                else if( H_o < h_1)
                {
                    Console.WriteLine(" h0 < h1 vo nghiem ");
                }
            } 
            else 
            {
               H_o = Z_res - Z_tail - Headloss;
            }

            Console.WriteLine(" gia tri ho la : {0:F4}", H_o);
            return H_o;

        }



        // ----------------------- ------------- MAIN FUNCTION  ----------------------------------- 
        public double BinarySearch( List<Data2D> data2Ds, List<Data3D> data3Ds, List<DataQETA> dataQETAs, double[] Pi0 ,double Z_tail, double Z_res , double HL , bool SingleOrMulti, double N, double n, double Q_MaxUnit)
        {
            double Ho = CalculateHPQ_HuuIch(data3Ds, Z_res, Z_tail, HL);
            double P0 = CalQplant(Pi0 , Ho);

            double Qo = Calculate_QoLow(data3Ds, Ho, P0);
            Console.WriteLine($"Qo : {Qo}");
            //double Qo = CalculateQovaQtop(data3Ds, data2Ds, P0, Ho, false);  // qoLow
            double Q_top = CalculateQtop(data3Ds, data2Ds,P0 , Ho, true); // QoTop
            if( Qo <= 0)
            {
                return 0;
            }

            double Q_Left = Qo;
            double Q_Right = Q_top;
            double e = 0.1;

            double Eta;
            double Q_mid = 0 ;
            double H_mid = 0;
            double Pmid = 0;
            double P_sosanh = P0 * 1000;
            int iterationCount = 0;
            int LoopN = 0;
            double curmid=0;
            while ( Math.Abs(P_sosanh - Pmid) > e )
            {
                
                iterationCount++; LoopN++;              
                Q_mid = ( Q_Left + Q_Right ) / 2;

                double HL_mid = 0;
                double Q_midunit = 0;  // q mid theo tung to may 

                if (SingleOrMulti == true) // chung duong dan 
                {

                    HL_mid = CalculateQHL_HLoss(data2Ds, Q_mid);
                    Q_midunit = Q_mid;
                }
                else if (SingleOrMulti == false) // duong dan rieng 
                {
                    if (N <= 0)
                    {
                        HL_mid = 0;
                        Console.WriteLine("So to may dang hoat dong la : " + HL_mid);


                    }
                    else {

                        Q_midunit = Q_mid / n;
                        HL_mid = CalculateQHL_HLoss(data2Ds, Q_midunit);

                    }

                }

                var maxQPoint = data3Ds.Aggregate((max, current) => current.Q > max.Q ? current : max);
                Q_MaxUnit = Q_MaxUnit / N;
                double QQmax = Q_midunit / Q_MaxUnit;
                Eta = CalculateQETA_Eta(dataQETAs, Q_midunit/Q_MaxUnit);
                H_mid = Z_res - Z_tail - HL_mid;
                Pmid = 9.8 * Q_mid * H_mid * Eta;

                Console.WriteLine($"{0,20}", '-');   
                Console.WriteLine("Q_mid = {0:F3}\n H_mid = {1:F3} \n Eta =  {2:F3}\n Pmid = {3:F4} \n ---  So lan lap {4:F2}", Q_mid, H_mid , Eta, Pmid, iterationCount);
                    
                Console.WriteLine("P_mid = {0:F3} \n P_o = {1:F3} \n  --- khoang cach :  {2:F3}", Pmid, P_sosanh, Math.Abs(P_sosanh - Pmid) );


                if( Math.Abs( P_sosanh - Pmid) <= e)
                {
                    break;
                 
                }
                if(Pmid < P_sosanh)
                {
                    Q_Left = Q_mid;
                    
                   
                } 
                else if( Pmid > P_sosanh)
                {
                    Q_Right = Q_mid;

                    
                }

               if(Q_mid == curmid)
                {
                    Console.WriteLine(iterationCount);
                    return Q_mid;
                } else if(Q_mid != curmid)
                {
                    curmid = Q_mid;
                }

            }

            return Q_mid;
        }



        // ---------------------------------- Functions Roles Private ------------
        public (double Q_day, double h_day) FindQ_dayAndH_day(List<Data3D> DataPoints, List<Data2D> DataPoint2D, double P0, double H0, bool dk)
        {
            var ListofP = DataPoints.Select(dp => dp.P).Distinct().OrderBy(p => p).ToList();

            var exactP0 = DataPoints.FirstOrDefault(dp => Math.Abs(dp.P - P0) < 0.00001);


            if (exactP0 != null) // truong hop P0 co trong du lieu 
            {
                var PointQolow = Calculate_QoLow(DataPoints, H0, P0);

                double Q_Day = 0;
                double Pi = P0 - 0.5;
                double Pi1 = P0 + 0.5;

                List<Data3D> ListPo = DataPoints.Where(x => x.P == P0).OrderBy(x => x.H).ToList();

                List<Data3D> ListPi = DataPoints.Where(x => x.P == Pi).OrderBy(x => x.H).ToList();

                List<Data3D> ListPi1 = DataPoints.Where(x => x.P == (Pi1)).OrderBy(x => x.H).ToList();

                /// 
                var Qotop = ListPo.LastOrDefault(dp => dp.Q != null && dp.Q < PointQolow && dp.H > H0);

                if (Q_Day != default)
                {
                    return (Q_Day, ListPo.Last().H);
                }
                else
                {
                    var listofH = DataPoints.Select(x => x.H).Distinct().OrderByDescending(h => h).ToList();

                    foreach (var p in listofH)
                    {
                        double epsilon = 0.000001;
                        int indexQItop = ListPi.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);

                        int indexQI1Top = ListPi1.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);


                        if (indexQItop >= 0 && indexQI1Top >= 0)
                        {
                            var Point_QItop = ListPi.ElementAt(indexQItop);

                            var Point_QI1Top = ListPi1.ElementAt(indexQI1Top);


                            if (Point_QItop != null && Point_QItop != null)
                            {

                                Q_Day = Point_QItop.Q + ((P0 - Point_QItop.P) * (Point_QI1Top.Q - Point_QItop.Q)) / (Point_QI1Top.P - Point_QItop.P);


                                if (Q_Day != 0)
                                {

                                    Console.WriteLine("tinh toan thanh cong");
                                    return (Q_Day, p);

                                }

                            }
                        }
                    }



                }


            }
            else if (exactP0 == null)
            {  // truong hop khong co po trong du lieu 

                var exactH0 = DataPoints.FirstOrDefault(dp => Math.Abs(dp.H - H0) < 0.00001);

                if (exactH0 != default)
                {

                    double Q_Day = 0;
                    double Pi = ListofP.LastOrDefault(dp => dp < P0);
                    double Pi1 = ListofP.FirstOrDefault(dp => dp > P0);

                    List<Data3D> ListPi = DataPoints.Where(x => Math.Abs(x.P - Pi) < 0.0001).OrderBy(x => x.H).ToList();
                    List<Data3D> ListPi1 = DataPoints.Where(x => Math.Abs(x.P - Pi1) < 0.0001).OrderBy(x => x.H).ToList();

                    var listofH = DataPoints.Select(x => x.H).Distinct().OrderByDescending(h => h).ToList();

                    foreach (var p in listofH)
                    {
                        double epsilon = 0.000001;
                        int indexQItop = ListPi.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);

                        int indexQI1Top = ListPi1.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);


                        if (indexQItop >= 0 && indexQI1Top >= 0)
                        {
                            var Point_QItop = ListPi.ElementAt(indexQItop);

                            var Point_QI1Top = ListPi1.ElementAt(indexQI1Top);


                            if (Point_QItop != null && Point_QI1Top != null)
                            {

                                Q_Day = Point_QItop.Q + ((P0 - Point_QItop.P) * (Point_QI1Top.Q - Point_QItop.Q)) / (Point_QI1Top.P - Point_QItop.P);


                                if (Q_Day != 0)
                                {

                                    Console.WriteLine("tinh toan thanh cong");
                                    return (Q_Day, p);

                                }

                            }
                        }
                    }

                }
                else
                {

                    double QoTop = 0;
                    double Pi = ListofP.LastOrDefault(dp => dp < P0);
                    double Pi1 = ListofP.FirstOrDefault(dp => dp > P0);

                    List<Data3D> ListPi = DataPoints.Where(x => Math.Abs(x.P - Pi) < 0.0001).OrderBy(x => x.H).ToList();
                    List<Data3D> ListPi1 = DataPoints.Where(x => Math.Abs(x.P - Pi1) < 0.0001).OrderBy(x => x.H).ToList();

                    var listofH = DataPoints.Select(x => x.H).Distinct().OrderBy(h => h).ToList();

                    foreach (var p in listofH)
                    {
                        double epsilon = 0.000001;
                        int indexQItop = ListPi.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);

                        int indexQI1Top = ListPi1.FindIndex(x => Math.Abs(x.H - (p)) < epsilon);


                        if (indexQItop >= 0 && indexQI1Top >= 0)
                        {
                            var Point_QItop = ListPi.ElementAt(indexQItop);

                            var Point_QI1Top = ListPi1.ElementAt(indexQI1Top);


                            if (Point_QItop != null && Point_QI1Top != null)
                            {

                                QoTop = Point_QItop.Q + ((P0 - Point_QItop.P) * (Point_QI1Top.Q - Point_QItop.Q)) / (Point_QI1Top.P - Point_QItop.P);


                                if (QoTop != 0)
                                {

                                    Console.WriteLine("tinh toan thanh cong");
                                    

                                }

                            }
                        }
                    }
                }


            }

            throw new Exception(" khong ton tai");
        }

        private double Checkduongdan(bool SingleOrMulti , double HL_mid, List<Data2D> data2Ds, double Q_mid, double Q_midunit , double n, double N)
        {

            if (SingleOrMulti == true) // chung duong dan 
            {

                HL_mid = CalculateQHL_HLoss(data2Ds, Q_mid);
                Q_midunit = Q_mid;
            }
            else if (SingleOrMulti == false) // duong dan rieng 
            {
                if (N <= 0)
                {
                    HL_mid = 0;
                    Console.WriteLine("So to may dang hoat dong la : " + HL_mid);


                }
                else
                {

                    Q_midunit = Q_mid / n;
                    HL_mid = CalculateQHL_HLoss(data2Ds, Q_midunit);
                    
                }

            }
            return HL_mid;
            
        }

        public double CalQplant(double[] Pi0, double H0)
        {
            double P0 = 0;
            double i = Pi0.Count();
            bool PiMaxSame = CheckPiMaxSame(Pi0);
            if (i == 1)
            {
                P0 = Pi0[0];
            }
            else if (i > 1)
            {
                if (PiMaxSame)
                {
                    P0 = Pi0.Sum();

                }
                else
                {
                    int numberOfDifferentUnits = FindNBOfDifferentUnit(Pi0);
                    P0 = CalculateP0ForDifferentUnits(Pi0, numberOfDifferentUnits);
                }
            }

            if (!IsRange(P0, H0))
            {
                HandleError1();
                return double.NaN;
            }

            Console.WriteLine("P0 : {0}", P0);
            return P0;
        }

        private double CalculateP0ForDifferentUnits(double[] Pi0, int j)
        {
            double P0 = 0;
            foreach (var pi in Pi0)
            {
                P0 += pi;
            }
            return P0;
        }

        private bool IsRange(double P0, double H0)
        {
            return P0 > 0 && H0 >= 0;
        }
        private double CalculateH_mid(double z_res, double Z_tail, double HL_mid)
        {
            return z_res - Z_tail - HL_mid;
        }

        private int FindNBOfDifferentUnit(double[] Pi0)
        {
            return Pi0.Distinct().Count();
        }
        private bool CheckPiMaxSame(double[] Pio)
        {
            return Pio.All(p => p == Pio[0]);
        }



        // ------------------------ Function excepted  ---------------

        public void PrintDataPoints(List<DataQETA> DataPoints)
        {
            int index = 1;
            foreach (var point in DataPoints)
            {
                Console.WriteLine($"DataPoint {index}: QQMax = {point.QQMax}, Eta = {point.Eta}");
                index++;
            }
        }

        private void HandleError1()
        {
            Console.WriteLine("Error 1: P0 hoac ho nam ngoai pham vi ");
        }
        
        public void ThrowException(string a)
        {
            throw new Exception(a);
        }
        private void HandleError2()
        {
            Console.WriteLine("Error 2: Noi suy cac phuong trinh khong thanh cong ");
        }









        //  Ham chua su dung

        public double Q_Tomay(List<Data3D> dataPoints, double[] Pi, int i, double[] Q)
        {
            double a; // luu luong cua nha may 
            double sumP = 0;
            double sumQ = 0;
            for (int j = 0; j < Pi.Length; j++)
            {
                if (Pi[j] == null)
                {

                }
                sumP = sumP + Pi[j];
                sumQ = sumQ + Q[j];
            }
            a = (sumQ * Pi[i]) / sumP;

            return a;
        }


        //  -------------------  Gardge Colletion ------------------------
        public DataHandle() { }
        ~DataHandle() { }
    }

}


// ham loi va chu thichs
/*
 *    double H1 = (double)(DataPoints.FirstOrDefault(dp => dp.P < P0)?.H);
            if (H1 == null)
            {
                throw new Exception(" khong tim duoc h1 ");
            }
            //Console.WriteLine("H1 : {0}", H1);
            
            int i = 2;
            double H3;

            var distinctListByH = DataPoints.GroupBy(dp => dp.H)
                                 .Select(g => g.First())
                                 .ToList();
            if (distinctListByH.Count >= 3)
            {
               
                var thirdH = distinctListByH.OrderBy(dp => dp.H).ElementAt(2).H;
            }
            else
            {
                Console.WriteLine("Danh sách không đủ 3 phần tử sau khi loại bỏ các phần tử trùng lặp.");
            }
            H3 = distinctListByH.OrderBy(dp => dp.H).ElementAt(2).H;

            //Console.WriteLine("H3 : {0}", H3);

            if ( H0 < H1 || H0 < H3)
            {
                throw new InvalidOperationException("khong phat duoc cong suat o muc nuoc hien tai ");
            }

            Data3D upperRight = null;  // Điểm chéo phải trên
            Data3D lowerLeft = null;   // Điểm chéo trái dưới
            Data3D Topleft = null;
            Data3D Topright = null;

            foreach (var point in DataPoints)
            {
                if (point.H < H0 && point.P > P0)
                {
                    if (upperRight == null || (point.H > upperRight.H && point.P < upperRight.P))
                    {
                        upperRight = point;
                    }
                }

                if (point.H > H0 && point.P < P0)
                {
                    if (lowerLeft == null || (point.H < lowerLeft.H && point.P > lowerLeft.P))
                    {
                        lowerLeft = point;
                    }
                }

            

            };

double Pi = P0 - 0.5;
double Pi1 = P0 + 0.5;

List<Data3D> ListPo = DataPoints.Where(x => x.P == P0).OrderBy(x => x.H).ToList();

List<Data3D> ListPi = DataPoints.Where(x => x.P == Pi).OrderBy(x => x.H).ToList();

List<Data3D> ListPi1 = DataPoints.Where(x => x.P == (Pi1)).OrderBy(x => x.H).ToList();

double QoTop = 0;
double QoLow = 0;

foreach (var p in ListPo)
{
    double epsilon = 0.000001;
    int indexQItop = ListPi.FindIndex(x => Math.Abs(x.H - (p.H - 0.1)) < epsilon);

    int indexQI1Top = ListPi1.FindIndex(x => Math.Abs(x.H - (p.H + 0.1)) < epsilon);


    if (indexQItop >= 0 && indexQI1Top == 0)
    {
        var Point_QItop = ListPi.ElementAt(indexQItop);

        var Point_QI1Top = ListPi1.ElementAt(indexQI1Top);


        if (Point_QItop != null && Point_QItop != null)
        {

            QoTop = Point_QItop.Q + ((P0 - Point_QItop.P) * (Point_QI1Top.Q - Point_QItop.Q)) / (Point_QI1Top.P - Point_QItop.P);

            QoLow = upperRight.Q + ((Point_QI1Top.P - P0) * (lowerLeft.Q - upperRight.Q)) / (upperRight.P - lowerLeft.P);
            if (QoLow != 0 && QoTop != 0)
            {
                Console.WriteLine("tinh toan thanh cong");
                break;
            }

        }
    }
}




if (dk == true)
{
    Console.WriteLine("Qotop : {0:F3}", QoTop);
    return QoTop;
}
else if (dk == false)
{
    Console.WriteLine("QoLow : {0:F3}", QoLow);
    return QoLow;
}*/