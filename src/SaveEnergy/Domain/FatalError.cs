namespace SaveEnergy.Domain;

public class FatalErrorException()
    : Exception("An error prevents executing the command. Please check the logs for more information.");