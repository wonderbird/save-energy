namespace SaveEnergy.Domain;

// TODO: FatalErrorException should describe the error in the message.
public class FatalErrorException()
    : Exception("An error prevents executing the command. Please check the logs for more information.");