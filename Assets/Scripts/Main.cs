using UnityEngine;


public class Main : MonoBehaviour 
{
    public GameObject ProjectorTemplate;
    public GameObject EmitterTemplate;
    public GameObject AccumulatorTemplate;
    public GameObject ReceiverTemplate;
    public GameObject EnergyTemplate;

	void Awake()
    {
	    Pico.ProjectorTemplate = ProjectorTemplate;
        Pico.EmitterTemplate = EmitterTemplate;
        Pico.AccumulatorTemplate = AccumulatorTemplate;
        Pico.ReceiverTemplate = ReceiverTemplate;
	    Pico.EnergyTemplate = EnergyTemplate;

        Pico.CycleLevels(WorldMap);
	}


   Level WorldMap()
    {
        var level = new Level(10, "Welcome to Pico,\n\nPico is a 3d puzzle game\nBring a coloured cube to an empty cube of the same colour. \n\nPress and drag 'Right Click' to rotate Around the world cube",1);

        level.AddEmitterAt(3, 6, 7, Direction.Right, Color.green);
        level.AddReceiverAt(7, 6, 7, LevelOne, Color.green);
        level.AddEmitterAt(3, 4, 7, Direction.Right, Color.blue);
        level.AddReceiverAt(7, 4, 7, LevelSix, Color.blue,Color.blue);
        level.AddEmitterAt(3, 2, 7, Direction.Right, Color.red);
        level.AddReceiverAt(7, 2, 7, LevelNine, Color.red,Color.red,Color.red);
                
        return level;
    }

    Level LevelOne()
    {
        var level = new Level(10, "Tutorial 1 - Red \n \nClick on the grid to create a Projector \nSend a Red Cell toward the empty Red cube(Receptor) \n \nPress 'Right Click' [A] to create a Projector", 2);
        
		level.AddEmitterAt(1, 5, 9, Direction.Backward, Color.red);
        level.AddReceiverAt(9, 5, 1, LevelTwo, Color.red);
        
        return level;
    }

    Level LevelTwo()
    {
        var level = new Level(10, "Tutorial 2 -  Green \n \nAdd Projectors to go around the faulty Projector \nSend a Green Cell to the Receptor \n \nPress 'Z' [B] to undo the last Projector", 3);
        
        level.AddEmitterAt(1, 5, 9, Direction.Right, Color.green);
        level.AddProjectorAt(9, 5, 3, Direction.Left);
        level.AddReceiverAt(9, 5, 1, LevelThree, Color.green);
        
        return level;
    }
    
    Level LevelThree()
    {
        var level = new Level(10, "Tutorial 3 - Cyan \n \nCreate a Projector where two Cells overlap \nMake a Cyan Cell from a Green Cell and a Blue Cell \n \nPress 'R' [back] to reset the level", 3);
           
        level.AddEmitterAt(1, 5, 9, Direction.Backward, Color.blue);
        level.AddEmitterAt(1, 5, 1, Direction.Forward, Color.green);
        level.AddReceiverAt(9, 5, 5, LevelFour, Color.cyan);
        
        return level;
    }
    
    Level LevelFour()
    {
        var level = new Level(10, "Tutorial 4 - White\n \nCreate a Projector where three Cells overlap \nMake a White Cell from a GC, a BC and a Red Cell(RC) \n \n\nPress 'Esc' [start] to return to Home", 3);
    	
        level.AddEmitterAt(1, 5, 9, Direction.Backward, Color.blue);
        level.AddEmitterAt(1, 1, 5, Direction.Up, Color.red);
        level.AddEmitterAt(1, 5, 1, Direction.Forward, Color.green);
        level.AddReceiverAt(9, 5, 5, LevelFive, Color.white);
        
        return level;
    }
    
    Level LevelFive()
    {
        var level = new Level(10, "Tutorial 5 - White\n \nCreate a Projector where three Cells overlap \nMake a White Cell from a Magenta, a Yellow and a Cyan Cell \n \n\nTap 'Spacebar' [X] to change the tempo of the game", 3);
    	
        level.AddEmitterAt(1, 7, 9, Direction.Down, Color.blue);
        level.AddEmitterAt(1, 3, 9, Direction.Up, Color.red);
        level.AddProjectorAt(1, 5, 9, Direction.Backward);
        
        level.AddEmitterAt(1, 7, 1, Direction.Down, Color.blue);
        level.AddEmitterAt(1, 3, 1, Direction.Up, Color.green);
        level.AddProjectorAt(1, 5, 1, Direction.Forward);

        level.AddEmitterAt(1, 1, 3, Direction.Forward, Color.green);
        level.AddEmitterAt(1, 1, 7, Direction.Backward, Color.red);
        level.AddProjectorAt(1, 1, 5, Direction.Up);
        
        level.AddProjectorAt(1, 5, 5, Direction.Right);

        
        level.AddReceiverAt(9, 5, 1, LevelSix, Color.white);
        
        return level;
    }

	Level LevelSix()
    {
        var level = new Level(10, "Inter 6 - Yellow Magenta\n \nPress 'W' [R] and 'S' [L] to move the grid up and down", 5);
        
        level.AddEmitterAt(1, 5, 9, Direction.Backward, Color.blue);
        level.AddEmitterAt(1, 1, 5, Direction.Up, Color.red);
        level.AddEmitterAt(1, 5, 1, Direction.Forward, Color.green);
        level.AddEmitterAt(1, 9, 5, Direction.Down, Color.red);
        //level.AddAccumulatorAt(5, 5, 5);
        level.AddReceiverAt(9, 5, 1, LevelSeven, Color.yellow, Color.magenta);

        return level;
    }

	Level LevelSeven()
    {
        var level = new Level(10, "Inter 7 - Blue Magenta\n \nAccumulators requires 2 cells to output one cell", 5);
        
    	level.AddEmitterAt(1, 5, 1, Direction.Right, Color.red);
       	level.AddEmitterAt(1, 5, 8, Direction.Right, Color.blue);
		level.AddAccumulatorAt(4, 5, 4);
		level.AddAccumulatorAt(6, 5, 4);
        level.AddReceiverAt(9, 5, 3, LevelEight, Color.blue, Color.magenta);
        
        return level;
    }

	Level LevelEight()
    {
        var level = new Level(10, "Inter 8 - Green Green Magenta\n \nThis puzzle requires 3 colours", 8);
        
    	level.AddEmitterAt(1, 5, 1, Direction.Right, Color.red);
       	level.AddEmitterAt(1, 6, 1, Direction.Right, Color.blue);
       	level.AddEmitterAt(1, 7, 1, Direction.Right, Color.green);
		level.AddAccumulatorAt(4, 5, 4);
		level.AddAccumulatorAt(6, 5, 4);
        level.AddReceiverAt(9, 5, 3, LevelNine, Color.green, Color.green, Color.magenta);
        
        return level;
    }

    Level LevelNine()
    {
        var level = new Level(10, "Inter 9 - White Magenta\n \nYou don't need to use all of the accumulators", 8);
        
    	level.AddEmitterAt(1, 5, 1, Direction.Right, Color.green);
    	level.AddEmitterAt(1, 5, 2, Direction.Right, Color.red);
       	level.AddEmitterAt(1, 5, 8, Direction.Right, Color.blue);
		level.AddAccumulatorAt(4, 5, 4);
		level.AddAccumulatorAt(5, 5, 4);
		level.AddAccumulatorAt(6, 5, 4);
        level.AddReceiverAt(9, 5, 3, LevelTen, Color.white, Color.magenta);

        return level;
    }
    
    
        Level LevelTen()
    {
        var level = new Level(10, "Hard 10 - Cyan Yellow\n \nWelcome to the hard difficulty", 5);
        
    	level.AddEmitterAt(1, 5, 1, Direction.Right, Color.green);
    	level.AddEmitterAt(1, 5, 3, Direction.Right, Color.red);
       	level.AddEmitterAt(1, 5, 5, Direction.Right, Color.blue);
		level.AddAccumulatorAt(5, 5, 3);
        level.AddReceiverAt(9, 5, 3, LevelEleven, Color.cyan, Color.yellow);

        return level;
    }
    
            Level LevelEleven()
    {
        var level = new Level(10, "Hard 11 - White Magenta Blue\n \nThis is easier than it looks", 7);
        
    	level.AddEmitterAt(1, 5, 1, Direction.Right, Color.green);
    	level.AddEmitterAt(1, 5, 3, Direction.Right, Color.red);
       	level.AddEmitterAt(1, 5, 5, Direction.Right, Color.blue);
		level.AddAccumulatorAt(5, 5, 3);
		level.AddAccumulatorAt(5, 3, 3);
        level.AddReceiverAt(9, 5, 3, LevelTwelve, Color.white, Color.magenta, Color.blue);

        return level;
    }
    
    
    Level LevelTwelve()
    {
        var level = new Level(10, "Hard 12 - Yellow Cyan Yellow Green\n \nLast Level", 12);
        
    	level.AddEmitterAt(1, 5, 1, Direction.Right, Color.red);
    	level.AddEmitterAt(1, 5, 9, Direction.Backward, Color.green);
    	level.AddEmitterAt(9, 5, 1, Direction.Forward, Color.green);
       	level.AddEmitterAt(9, 5, 9, Direction.Left, Color.blue);
        level.AddProjectorAt(8, 2, 8, Direction.Left);
        level.AddProjectorAt(2, 2, 2, Direction.Right);
        level.AddProjectorAt(8, 2, 2, Direction.Forward);
        level.AddProjectorAt(2, 2, 8, Direction.Backward);
		level.AddAccumulatorAt(3, 2, 5);
		level.AddAccumulatorAt(5, 2, 3);
		level.AddAccumulatorAt(5, 2, 7);
		level.AddAccumulatorAt(7, 2, 5);
        level.AddReceiverAt(5, 5, 5, LevelCredit, Color.yellow, Color.cyan, Color.yellow);

        return level;
    }



    Level LevelCredit()
    {
        var level = new Level(10, "Thank you for playing\n\nGame Design by Devine Lu Linvega\nPrograming by Renaud Bedard", 1);
        
    	        // White Output
		level.AddEmitterAt(1, 1, 5, Direction.Up, Color.red);
		level.AddEmitterAt(3, 1, 5, Direction.Up, Color.green);
		level.AddEmitterAt(4, 2, 5, Direction.Up, Color.green);
		level.AddEmitterAt(6, 2, 5, Direction.Up, Color.blue);
		level.AddEmitterAt(7, 1, 5, Direction.Up, Color.blue);
		level.AddEmitterAt(9, 1, 5, Direction.Up, Color.red);
        level.AddProjectorAt(1, 3, 5, Direction.Right);
        level.AddProjectorAt(3, 3, 5, Direction.Left);
        level.AddProjectorAt(4, 5, 5, Direction.Right);
        level.AddProjectorAt(6, 5, 5, Direction.Left);
        level.AddProjectorAt(7, 3, 5, Direction.Right);
        level.AddProjectorAt(9, 3, 5, Direction.Left);
        level.AddProjectorAt(2, 3, 5, Direction.Up);
        level.AddProjectorAt(5, 5, 5, Direction.Up);
        level.AddProjectorAt(8, 3, 5, Direction.Up);
        level.AddProjectorAt(2, 7, 5, Direction.Right);
        level.AddProjectorAt(8, 7, 5, Direction.Left);
        level.AddProjectorAt(5, 7, 5, Direction.Up);
        level.AddReceiverAt(5, 9, 5, WorldMap, Color.white);

        return level;
    }



    void Update()
    {
        Pico.Update();
    }
}
