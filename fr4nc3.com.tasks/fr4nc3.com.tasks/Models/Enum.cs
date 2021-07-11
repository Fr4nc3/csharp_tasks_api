namespace fr4nc3.com.tasks.Models
{
    /// <summary>
    // Error Code Enum
    /// </summary>
    public enum ErrorCode
    {
        EntityAlreadyExist = 1,
        ParameterTooLarge = 2,
        ParameterRequired = 3,
        MaxEntitiesReached = 4,
        EntityNoFound = 5,
        ParameterTooSmall = 6,
        ParameterNoValid = 7,
        EntityNoInserted = 8,
        EntityNoUpdated = 9,
        EntityNoDeleted = 10,
        ServerError = 11
    }
}
