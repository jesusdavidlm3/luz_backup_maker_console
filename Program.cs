﻿using System;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;

char[] abc = new char[26];  //Lista de todas las posibles unidades
List<char> drives = [];     //Lista de las unidades detectadas
char origin;                //origen seleccionado
char destiny;               //Destino seleccionado
int listNumber = 1;         //Contador para listas
bool soFound = false;       //Variable que dice si existe un SO en el disco origen
string backupName;          //Nombre de la carpeta destino del respaldo
List<string> errorList = [];

string[] includedDirectories = {    //Carpetas que se copiaran desde todos los usuarios
    "Documents",
    "Desktop",
    "Downloads",
    "Music",
    "Videos",
    "Pictures"
};
List<string> excludedFiles = 
[
    ".exe", ".mp3", ".mp4", ".ini", ".ink", ".tmp", ".lnk", ".log", ".bak", ".old", ".dll", ".win", ".iso",
    ".img", ".asp", ".msp", ".cfg", ".cat", ".bin", ".mkv", ".efi", ".p7b", ".inf", ".msi", ".bmp", ".mui",
    ".dat", ".man"
];

void getDrives(){
    drives = [];
    foreach(char item in abc)   //Detectamos que unidades existen y las guardamos en la lista de unidades detectadas.
    {
        if(Directory.Exists($"{item}:\\"))
        {
            drives.Add(item);
        }
    }
}

void copyFiles(string oDirectory, string dDirectory){
    foreach(string file in Directory.GetFiles(oDirectory))
    {
        string fileName = Path.GetFileName(file);
        string extension = Path.GetExtension(file);
        if(!excludedFiles.Contains(extension))
        {
            Console.WriteLine($"Se esta copiando {file}");
            File.Copy(file, $"{dDirectory}\\{fileName}", true);
            Console.WriteLine($"Se copio: {file}");
        }
        else
        {
            Console.WriteLine($"No se copio: {file}");
        }
    }
}

void copyFolders(string oDirectory, string dDirectory){
    string folderName = Path.GetFileName(oDirectory);
    try{
        Directory.CreateDirectory($"{dDirectory}\\{folderName}");
        Console.WriteLine($"Se encontro la carpeta: {oDirectory}");
        copyFiles(oDirectory, $"{dDirectory}\\{folderName}");
        string[] folders = Directory.GetDirectories(oDirectory);
        if(folders.Length > 0){
            foreach(string folder in folders){
                copyFolders(folder, $"{dDirectory}\\{folderName}");
            }
        }
    }catch(Exception ex){
        Console.WriteLine("Error al copiar un archivo");
        errorList.Add(ex.Message);
    }
}

for(int i = 0; i < 26;i++)  //Llenamos la lista con todas las letras de posibles unidades
{
    abc[i] = (char)('A'+i);
}
do{             //Este bloque se ejecuta mientras el origen y el destino sean el mismo disco
    getDrives();
    Console.WriteLine("Seleccione una unidad de origen:");
    foreach(char item in drives)    //Imprimimos las opciones
    {
        Console.WriteLine($"{listNumber}. Disco {item}");
        listNumber++ ;
    }
    listNumber = 1;             
    origin = char.ToUpper(char.Parse(Console.ReadLine()));  //Leemos la respuesta del usuario
    soFound = Directory.Exists($"{origin}:\\Users");


    while((!drives.Contains(origin)) || (soFound == false)) //Si la respuesta no es valida repetimos el proceso hasta que lo sea
    {
        Console.Clear();
        if(!drives.Contains(origin))
        {
            Console.WriteLine($"La unidad {origin} no se encuentra en la lista");
        }else if(soFound == false)
        {
            Console.WriteLine("Este disco no contiene un sistema operativo Windows");
        }

        getDrives();
        Console.WriteLine("Intente con una de las siguientes opciones:");
        foreach(char item in drives)
        {
            Console.WriteLine($"{listNumber}. Disco {item}");
            listNumber++;
        }
        listNumber = 1;
        origin = char.ToUpper(char.Parse(Console.ReadLine()));
        soFound = Directory.Exists($"{origin}:\\Users");
    }
    Console.Clear();        

    Console.WriteLine($"Ah seleccionado el disco {origin} como disco de origen");
    Console.WriteLine("Seleccione un disco de destino");

    getDrives();
    foreach(char item in drives)
    {
        Console.WriteLine($"{listNumber}. Disco {item}");
    }
    listNumber = 1;
    destiny = char.ToUpper(char.Parse(Console.ReadLine()));     //Leemos la respuesa de destino del usuario

    while(!drives.Contains(destiny))    //Si el destino no es valido se reasigna hasta que lo sea
    {
        Console.Clear();
        Console.WriteLine($"La unidad {destiny} no se encuentra en la lista");
        Console.WriteLine("Intente con una de las siguientes opciones:");
        getDrives();
        foreach(char item in drives)
        {
            Console.WriteLine($"{listNumber}. Disco {item}");
        }
        listNumber = 1;
        destiny = char.ToUpper(char.Parse(Console.ReadLine()));
    }
    Console.Clear();
    if(destiny == origin)
    {
        Console.WriteLine("Ah Seleccionado el mismo disco como origen y destino");
        Console.WriteLine("Por favor seleccione discos distintos");
    }
}while(destiny == origin);

Console.WriteLine($"Ah seleccionado el disco {origin} como origen y {destiny} como destino");

string[] excludedDirectories = {    //carpetas predeterminadas de windows que no se respaldaran
    $"{origin}:\\Users\\All Users",
    $"{origin}:\\Users\\Default",
    $"{origin}:\\Users\\Public",
    $"{origin}:\\Users\\Default User"
};

Console.WriteLine("De un nombre al respaldo");
backupName = Console.ReadLine();

Console.WriteLine("Escriba las extensiones de archivo que desea excluir separados por espacios");
Console.WriteLine("Por defecto se excluyen: .exe, .mp3, .mp4, .ini, .ink, .tmp, .lnk, .log, .bak, .old, .dll, .win, .ISO, .img, .asp, .MSP, .cfg, .cat, .bin, .mkv, .EFI, .p7b, .inf, .MSI, .BMP, .mui, .dat, .man");
List<string> rawExcludedFiles = new List<string>(Console.ReadLine().Split(' '));
foreach(string ext in rawExcludedFiles){
    excludedFiles.Add(ext);
}

List<string> users = new List<string>(Directory.GetDirectories($"{origin}:\\Users"));   //Obtenemos las capetas de usuario

users = users.Where(i => !excludedDirectories.Contains(i)).ToList();

string destinyDirectory = $"{destiny}:\\{backupName}";  //Carpeta que guaradara el respaldo
Directory.CreateDirectory(destinyDirectory);     //Creamos el directorio destino del respaldo

foreach(string user in users)
{
    string userFolderName = Path.GetFileName(user);
    string destinyUserFolder = $"{destiny}:\\{backupName}\\{userFolderName}";
    Directory.CreateDirectory(destinyUserFolder);
    foreach(string directory in includedDirectories)
    {
        copyFolders($"{user}\\{directory}", destinyUserFolder);
    }
}

if(errorList.Count != 0 )
{
    Console.WriteLine("Se enconraron los siguientes errores:");
    foreach(string err in errorList)
    {
        Console.WriteLine(err);
    }
    Console.WriteLine("Si desconoce la ubicacion de estas carpetas ignorelas. es posible que sean enlaces simbolicos a otras bibliotecas");
}

Console.WriteLine("Ejecucion finalizada, presione una tecla para Cerrar");
Console.ReadLine();