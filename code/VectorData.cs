//  all vectors used for the prediction


using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

class VectorData
{

    //  circles
        VecPoint mapCenter;
        VecPoint firstCircle;  
        VecPoint secondCircle; 
        VecPoint thirdCircle;           //  unknown
        VecPoint fourthCircle;          //  unknown
        VecPoint fifthCircle;           //  unknown
        

    
    //  base vectors
        VecPoint vec_centerToFirst;
        VecPoint vec_firstToSecond;
        VecPoint vec_secondToThird;     //  unknown
        VecPoint vec_thirdToFourth;     //  unknown
        VecPoint vec_fourthToFinal;     //  unknown, the thing we wan't to find out
    //  calculated vectors
        VecPoint vec_centerToEdge;
        VecPoint vec_firstToSecond_mid; //  todo
    //  combined vectors
        VecPoint vec_seedPull;          //  todo
    //  tangent method / LinBoBo methods
        VecPoint A, B, C, D, E, F, G, J, I;        //  skipping H since its useless anyway

        VecPoint vec_AtoC;  //  same as vec_centerToEdge
        VecPoint vec_BtoE;  
        VecPoint vec_BtoD;
        VecPoint vec_EtoF;  //  same as vec_AtoC
        VecPoint vec_FtoG;  //  the hard one
        VecPoint vec_JtoI;

        VecPoint vec_DtoJ;

    //
        private bool isSourced = false;


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
            VecPoint vec_centerToEdge = new VecPoint(-(int)vec.X, -(int)vec.Y); 

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

            vec_FtoG = (VecPoint.GetClosestPoint(D, E, F) - F);
            vec_FtoG = vec_FtoG + vec_FtoG;
            vec_JtoI = new VecPoint() - vec_FtoG;
            G = F + vec_FtoG;
            J = D + vec_DtoJ;
            I = J + vec_JtoI;

            //vec_FtoG = G - F;
            //  todo, find G

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