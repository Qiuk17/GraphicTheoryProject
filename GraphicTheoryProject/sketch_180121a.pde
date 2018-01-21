BufferedReader reader;
String line;

void setup()
{
  reader = createReader("res.txt");
  size(2100, 1200);
  background(255);
  frameRate(60);
  while(true){
    try
  {
    line = reader.readLine();
  }
  catch(IOException e)
  {
    e.printStackTrace();
    line = null;
  }
  if(line != null)
  {
    String[] pieces = split(line, ' ');
    int s = pieces.length;
    if(s == 7)
    {
      int x1 = int(pieces[0]);
      int y1 = int(pieces[1]);
      int x2 = int(pieces[2]);
      int y2 = int(pieces[3]);
      int r = int(pieces[4]);
      int g = int(pieces[5]);
      int b = int(pieces[6]);
      stroke(r,g,b);
      line(x1+20, y1+20, x2+20, y2+20);
    }
    else if(s == 6)
    {
      int x = int(pieces[0]);
      int y = int(pieces[1]);
      int R = int(pieces[2]);
      int r = int(pieces[3]);
      int g = int(pieces[4]);
      int b = int(pieces[5]);
      fill(r,g,b); stroke(r,g,b);
      ellipse(x+20, y+20, R, R);
    }
  }
  else
  {
    //noLoop();break;
    break;
  }}
}

void draw()
{
   noLoop();
}