using System;

namespace CrawData.Models;

public class Operator
{
    public string OperatorId { get; set; }
    public string OperatorName { get; set;}
    public string OperatorIcon { get; set;}
    public string UnknownInformation { get; set;}
    public Ability Ability{ get; set;} = new Ability();
    public List<PrimaryWeapon> PrimaryWeapon { get; set;} = new List<PrimaryWeapon>();
    public List<SecondaryWeapon> SecondaryWeapon { get; set;} = new List<SecondaryWeapon>();
    public List<Gadget> Gadgets { get; set;} = new List<Gadget>();
    public Skill Skill {get; set;}
    public string Side { get; set;}
    public string Squad { get; set;}
    public string SquadIcon { get; set;}    
    public int Health { get; set;}
    public int Speed { get; set;}
    public int Difficulty{ get; set;}
    public string RealName { get; set;}
    public string DateofBirth { get; set;}  
    public string PlaceofBirth { get; set;}
    public string Biography { get; set;}

}
