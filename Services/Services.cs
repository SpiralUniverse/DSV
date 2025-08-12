using System;

namespace DSV.Services;

public class Services
{
    public static readonly UpdateService UpdateLoop = new UpdateService(TimeSpan.FromMilliseconds(16));
}