using System;
public class People
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Profession { get; set; }
    
    public People()
    {
        this.Name = "";
        this.Phone = "";
        this.Email = "";
        this.Profession = "";
    }

    // Constructor that takes one argument. 
    public People(string n, string c, string e, string p )
    {
        this.Name = n;
        this.Phone = c;
        this.Email = e;
        this.Profession = p;
    }

    public string getAll(People p) {
        string retString = p.Name + "||" + p.Phone + "||"+ p.Email +"||"+ p.Profession;
        retString.ToString();
        return retString;
    }
}
