namespace Halogen.DbModels;

public partial class Currency {
    public Currency(string id, string name, string code, string symbol, bool symbolPosition) {
        Id = id;
        Name = name;
        Code = code;
        Symbol = symbol;
        SymbolPosition = symbolPosition;
        
        LocalityPrimaryCurrencies = new HashSet<Locality>();
        LocalitySecondaryCurrencies = new HashSet<Locality>();
    }
}