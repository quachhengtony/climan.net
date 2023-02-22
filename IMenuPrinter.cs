namespace Climan.Model;
internal interface IMainMenuPrinter
{
    int PrintMainMenu(bool canCancel, params string[] options);
}

internal interface IRepositoryMenuPrinter
{
    int PrintRepositoryMenu(bool canCancel);
}
