using Halogen.DbModels;

namespace HaloUnitTest.Mocks;

internal record DataMocks {
    internal Account[] Accounts { get; }
    internal Role[] Roles { get; }
    internal AccountRole[] AccountRoles { get; }
    internal Preference[] Preferences { get; }
    internal Profile[] Profiles { get; }
    internal Currency[] Currencies { get; }
    internal Locality[] Localities { get; }
    internal LocalityDivision[] LocalityDivisions { get; }
    internal Address[] Addresses { get; }
    internal ProfileAddress[] ProfileAddresses { get; }
    internal Challenge[] Challenges { get; }
    internal ChallengeResponse[] ChallengeResponses { get; }
    internal TrustedDevice[] TrustedDevices { get; }

    public DataMocks() {
        Accounts = Enumerable.Range(0, 35).Select(_ => new Fakers.AccountFaker().Generate()).ToArray();
        Roles = Fakers.Roles;
        
        var customers = Accounts.Take(8).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Customer").Id).Generate()).ToArray();
        var customerSupports = Accounts.Skip(8).Take(3).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Customer Support").Id).Generate()).ToArray();
        var retailStaffs = Accounts.Skip(11).Take(3).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Retail Staff").Id).Generate()).ToArray();
        var retailManagers = Accounts.Skip(14).Take(2).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Retail Manager").Id).Generate()).ToArray();
        var warehouseStaffs = Accounts.Skip(16).Take(3).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Warehouse Staff").Id).Generate()).ToArray();
        var warehouseManagers = Accounts.Skip(19).Take(2).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Warehouse Manager").Id).Generate()).ToArray();
        var accountants = Accounts.Skip(21).Take(3).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Accountant").Id).Generate()).ToArray();
        var financialManagers = Accounts.Skip(24).Take(2).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Financial Manager").Id).Generate()).ToArray();
        var marketingStaffs = Accounts.Skip(26).Take(3).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Marketing Staff").Id).Generate()).ToArray();
        var marketingManagers = Accounts.Skip(29).Take(2).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Marketing Manager").Id).Generate()).ToArray();
        var operationManagers = Accounts.Skip(31).Take(2).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Operation Manager").Id).Generate()).ToArray();
        var administrators = Accounts.Skip(33).Select(a => new Fakers.AccountRoleFaker(a.Id, Roles.Single(r => r.Name == "Administrator").Id).Generate()).ToArray();
        AccountRoles = customers.Concat(customerSupports).Concat(retailStaffs).Concat(retailManagers).Concat(warehouseStaffs).Concat(warehouseManagers).Concat(accountants)
            .Concat(financialManagers).Concat(marketingStaffs).Concat(marketingManagers).Concat(operationManagers).Concat(administrators).ToArray();

        // Preferences = Accounts.Select(a => new Fakers.PreferenceFaker(a.Id).Generate()).ToArray();
        // Profiles = Accounts.Select(a => new Fakers.ProfileFaker(a.Id).Generate()).ToArray();
        // Currencies = Fakers.Currencies;
        // Localities = Fakers.Localities;
        // LocalityDivisions = Localities.SelectMany(l => Enumerable.Range(0, 5).Select(_ => new Fakers.LocalityDivisionFaker(l.Id).Generate()).ToArray()).ToArray();
        //
        // var localityIds = Localities.Select(l => l.Id).ToArray();
        // var divisionByLocalityIds =
        //     LocalityDivisions.Select(d => new { dId = d.Id, lId = d.LocalityId }).ToDictionary(x => x.lId, x => x.dId);
        // Addresses = Enumerable.Range(0, 40).Select(_ => new Fakers.AddressFaker(localityIds, divisionByLocalityIds).Generate()).ToArray();
        //
        // ProfileAddresses = Accounts.Take(8).SelectMany((a, i) => {
        //     var profileId = Profiles.Single(p => p.AccountId == a.Id).Id;
        //     return Addresses.Skip(i*5).Take(5).Select(a => new Fakers.ProfileAddressFaker(profileId, a.Id).Generate()).ToArray();
        // }).ToArray();
        //
        // var customerIds = Accounts.Skip(33).Select(a => a.Id).ToArray();
        // Challenges = Enumerable.Range(0, 10).Select(_ => new Fakers.ChallengeFaker(customerIds).Generate()).ToArray();
        //
        // var challengeIds = Challenges.Select(c => c.Id).ToArray();
        // ChallengeResponses = Accounts.Take(8).Select(a => new Fakers.ChallengeResponseFaker(a.Id, challengeIds).Generate()).ToArray();
        //
        // TrustedDevices = Accounts.SelectMany(a => Enumerable.Range(0, 3).Select(_ => new Fakers.TrustedDeviceFaker(a.Id).Generate()).ToArray()).ToArray();

        SetVirtualProperties();
    }

    private void SetVirtualProperties() {
        Accounts.ToList().ForEach(a => {
            var roleIds = AccountRoles.Where(ar => ar.AccountId == a.Id).Select(ar => ar.RoleId);
            a.Roles = Roles.Where(r => roleIds.Contains(r.Id)).ToHashSet();

            // a.Preferences = Preferences.Where(p => p.AccountId == a.Id).ToHashSet();
            // a.Profiles = Profiles.Where(p => p.AccountId == a.Id).ToHashSet();
            // a.ChallengeResponses = ChallengeResponses.Where(cr => cr.AccountId == a.Id).ToHashSet();
            //
            // var challengeIds = a.ChallengeResponses.Select(cr => cr.ChallengeId);
            // a.Challenges = Challenges.Where(c => challengeIds.Contains(c.Id)).ToHashSet();
            //
            // a.TrustedDevices = TrustedDevices.Where(td => td.AccountId == a.Id).ToHashSet();
        });
        
        Roles.ToList().ForEach(r => {
            r.AccountRoles = AccountRoles.Where(ar => ar.RoleId == r.Id).ToHashSet();
        });
    }
};