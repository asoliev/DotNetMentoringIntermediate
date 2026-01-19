namespace GameOfLife;

class Cell(int row, int column, int age, bool alive)
{
    public int PositionX { get; set; } = row * 5;
    public int PositionY { get; set; } = column * 5;
    public int Age { get; set; } = age;

    public bool IsAlive { get; set; } = alive;
}