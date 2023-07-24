#pragma warning disable CA1416

//  all vectors used for the prediction


using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

class VectorData
{

    //  circles
        public VecPoint mapCenter;
        public VecPoint firstCircle;  
        public VecPoint secondCircle; 
        public VecPoint thirdCircle;           //  unknown
        public VecPoint fourthCircle;          //  unknown
        public VecPoint fifthCircle;           //  unknown
        
    //  base vectors
        public VecPoint vec_centerToFirst;
        public VecPoint vec_firstToSecond;
        public VecPoint vec_secondToThird;     //  unknown
        public VecPoint vec_thirdToFourth;     //  unknown
        public VecPoint vec_fourthToFinal;     //  unknown, the thing we wan't to find out
    //  calculated vectors
        public VecPoint vec_centerToEdge;
        public VecPoint vec_firstToSecond_mid; //  todo
    //  combined vectors
        public VecPoint vec_seedPull;          //  todo
    //  tangent method / LinBoBo methods
        public VecPoint A, B, C, D, E, F, G, J, I, M;        //  skipping H since its useless anyway

        public VecPoint vec_AtoC;  //  same as vec_centerToEdge
        public VecPoint vec_BtoE;  
        public VecPoint vec_BtoD;
        public VecPoint vec_EtoF;  //  same as vec_AtoC
        public VecPoint vec_FtoG;  //  the hard one
        public VecPoint vec_JtoI;
        public VecPoint vec_DtoJ;


    //  angles
        public double diffAngle;
        public double pullAngle;
        public double pullAngle_high;
        public double pullAngle_low;

    //  other
        public double pullLength;

    //
        public bool isSourced = false;

    public VecPoint[] RingCenters 
    {   get
    {
        return new VecPoint[] 
        {
            this.firstCircle,
            this.secondCircle,
            this.thirdCircle,
            this.fourthCircle,
            this.fifthCircle
        };
    }
    }


    //  when loading from a source where everything is known
    public VectorData(VecPoint[] source)
        : this(source[0], source[1])
    {


        
        this.thirdCircle = source[2];
        this.fourthCircle = source[3];
        this.fifthCircle = source[4];

        vec_secondToThird = thirdCircle - secondCircle;
        vec_thirdToFourth = fourthCircle - thirdCircle;
        vec_fourthToFinal = fifthCircle - fourthCircle;

        this.isSourced = true;


    }

    public VectorData(Point firstCircle, Point secondCircle)
        : this(new VecPoint(firstCircle), new VecPoint(secondCircle))
    {}

    //  for predictions
    public VectorData(VecPoint firstCircle, VecPoint secondCircle)
    {
        this.mapCenter = new VecPoint(DataSource.mapResolution.Width / 2, DataSource.mapResolution.Height / 2);
        this.firstCircle = firstCircle;
        this.secondCircle = secondCircle;

        this.vec_centerToFirst = firstCircle - mapCenter;
        this.vec_firstToSecond = secondCircle - firstCircle;

        //  edge vector
            Vector2 vec = new Vector2(vec_centerToFirst.X, vec_centerToFirst.Y);
            float circleRadius = (float)DataSource.we_ringRadius[1];
            vec = new Vector2(vec.X * ((circleRadius - vec.Length() ) / circleRadius), vec.Y * ((circleRadius - vec.Length())  / circleRadius));
            vec_centerToEdge = new VecPoint(-(int)vec.X, -(int)vec.Y); 

        //  Tangent method

            vec_AtoC = vec_centerToEdge;
            vec_BtoD = vec_firstToSecond;
            vec_BtoE = vec_BtoD + vec_BtoD;
            vec_EtoF = vec_centerToEdge;
            vec_DtoJ = new VecPoint() - vec_AtoC;
           
            



            A = mapCenter;
            B = firstCircle;
            C = A + vec_AtoC;
            D = secondCircle;
            E = B + vec_BtoE;
            F = E + vec_EtoF;
            M = D + vec_BtoD / 2;

            vec_FtoG = (VecPoint.GetClosestPoint(D, E, F) - F);
            vec_FtoG = vec_FtoG + vec_FtoG;
            vec_JtoI = new VecPoint() - vec_FtoG;
            G = F + vec_FtoG;
            J = D + vec_DtoJ;
            I = J + vec_JtoI;


        //  important angles
            VecPoint c1 = firstCircle;
            VecPoint c2 = secondCircle;

            this.diffAngle = Math.Abs(mapCenter.AngleTo(c1) - mapCenter.AngleTo(c2));
            this.pullAngle = c1.AngleTo(mapCenter, c2);
            if (this.pullAngle > Math.PI)
            {
                this.pullAngle_high = this.pullAngle;
                this.pullAngle_low = this.pullAngle - Math.PI;
            }
            else
            {
                this.pullAngle_high = this.pullAngle + Math.PI;
                this.pullAngle_low = this.pullAngle;
            }
        
        //  other
            this.pullLength = (this.firstCircle - this.secondCircle).Length;
            

    }

    public void Draw(Bitmap canvas)
    {

        Pen pen = new Pen(Color.Red);
        pen.Color = Color.Blue;
        Graphics formGraphics = Graphics.FromImage(canvas);


        //  edge vector
        VecPoint.DrawVector(canvas, A, C, Color.Red);

        //  base
        VecPoint.DrawVector(canvas, mapCenter, firstCircle, Color.Blue);
        VecPoint.DrawVector(canvas, firstCircle, secondCircle, Color.Blue);
        if (isSourced)
        {
            VecPoint.DrawVector(canvas, secondCircle, thirdCircle, Color.Blue);
            VecPoint.DrawVector(canvas, thirdCircle, fourthCircle, Color.Blue);
            VecPoint.DrawVector(canvas, fourthCircle, fifthCircle, Color.Blue);
        }

        //  tangent method
        //Predictor.DrawVector(canvas, B, C, Color.Green);
        VecPoint.DrawVector(canvas, D, E, Color.Green);
        VecPoint.DrawVector(canvas, E, F, Color.Green);
        VecPoint.DrawVector(canvas, F, G, Color.Green);
        VecPoint.DrawVector(canvas, D, J, Color.Green);
        VecPoint.DrawVector(canvas, F, J, Color.Green);
        VecPoint.DrawVector(canvas, J, I, Color.Green);

    }

    

}