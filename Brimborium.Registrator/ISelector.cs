namespace Brimborium.Registrator;

/// <summary>
/// public for testing only - otherwise 'you' do not need it
/// </summary>
public interface ISelector {
    void Populate(RegistrationStrategy registrationStrategy, ISelectorTarget selectorTarget);
}

/// <summary>
/// public for testing only - otherwise 'you' do not need it
/// </summary>
public interface ISelectorTarget {
    /// <summary>
    /// A list of all found ServicePopulation.
    /// </summary>
    List<ServicePopulation> ListServicePopulation { get; }
    
    /// <summary>
    /// internal
    /// </summary>
    void AddServicePopulation(ServicePopulation value);

    /// <summary>
    /// A list of all found OptionPopulation.
    /// </summary>
    List<OptionPopulation> ListOptionPopulation { get; }

    /// <summary>
    /// internal
    /// </summary>
    void AddOptionPopulation(OptionPopulation value);
}
