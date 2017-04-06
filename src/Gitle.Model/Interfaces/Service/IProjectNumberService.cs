namespace Gitle.Model.Interfaces.Service
{
    public interface IProjectNumberService
    {
        (int Initial, int Service, int Internal) GetNextProjectNumbers();
    }
}