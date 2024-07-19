using HelperLibrary.Shared.Helpers;

namespace HaloUnitTest.HelperLibraryTests;

[TestFixture]
public sealed class DataValidatorsTest {
    
    [Test]
    public void Test_VerifyEmailAddress() {
        // Test for invalid email address formats
        var emailAddresses = new[] {
            "some@th1ng.a",
            "s0_me@th-ing.ab.c",
            "so-me@th_1ng.ab.cd.ef",
            "some-thing@e_mail",
            "so.me@@thing.ab",
            "some@@thing.ab.cd",
            "some@thing.a-b",
            "some@thing.a_b",
            "some@thing.a1",
            "some@thing.ab.c-d",
            "some@thing.ab.c_d",
            "some@thing.ab.c1",
            "some@thing@else.com",
            "s0me.th-ing_else",
            "s0me@th1ng.abcde",
            "so_me@th-ing.ab.cdefg",
        };

        var results = emailAddresses.Select(emailAddress => emailAddress.VerifyEmailAddress()).ToList();
        var expect = new List<string> { "Email Address format seems to be invalid." };
        results.ForEach(result => Assert.That(result, Is.EquivalentTo(expect)));
        
        // Test for short email address
        emailAddresses = [
            "a@b.cdef",
            "a1@b2.cd",
            "a.1@b2.cd",
            "a-1@b.cd",
            "a_1@b.cde",
            "a@b-1.cd",
            "a@b_1.cd",
            "a@b.cd.ef",
        ];
        
        results = emailAddresses.Select(emailAddress => emailAddress.VerifyEmailAddress()).ToList();
        expect = ["Email Address is too short. Min 10, max 100 characters."];
        results.ForEach(result => Assert.That(result, Is.EquivalentTo(expect)));

        // Test for long email address
        emailAddresses = [
            "very-long-very-long-very-long-very-long-very-long-very-long-very-long-very-long-very-long@ex5mple.abc",
            "very-long_very-long.very-long_very-long.very-long_very-long.very-long_very-long.very-long@example-mail.abcd.efgh",
            "very-l0ng-very-20ng-very-long-very-long-very-long-very-long-very-long-very-long-very-long@example_mail.abc.de",
            "very-l0ng-very-20ng-very-long-v3ry_long.very-long-very-long-very-long-very-long-very-long@exa111ple.abcd.ef",
        ];
        
        results = emailAddresses.Select(emailAddress => emailAddress.VerifyEmailAddress()).ToList();
        expect = ["Email Address is too long. Min 10, max 100 characters."];
        results.ForEach(result => Assert.That(result, Is.EquivalentTo(expect)));
        
        // Test for adjacent special characters
        emailAddresses = [
            "s-ome__th1ng@e_ma1l.abcd.ef",
            "s0me-_thing@do-main.ab.cdef",
            "som3th1ng@do--main.ab",
            "s0mth1ng-@domain.com",
            "s0m3th1ng_@d0-ma1n.ab.cde",
            "8ome@th__ng.com",
        ];
        
        results = emailAddresses.Select(emailAddress => emailAddress.VerifyEmailAddress()).ToList();
        expect = ["Email Address should not contain adjacent special characters."];
        results.ForEach(result => Assert.That(result, Is.EquivalentTo(expect)));
        
        // Test for leading and trailing special characters
        emailAddresses = [
            "-ome_th1ng@e_ma1l.abcd.ef",
            "_som3th1ng@d0main.ab.cdef",
            ".s0mth1ng@domain.com",
        ];
        
        results = emailAddresses.Select(emailAddress => emailAddress.VerifyEmailAddress()).ToList();
        expect = ["Email Address should not start or end with special characters."];
        results.ForEach(result => Assert.That(result, Is.EquivalentTo(expect)));
        
        // Test for valid email address
        emailAddresses = [
            "s0m3@th1ng.ab.cdef",
            "s0.m3@th-in9.abcd.ef",
            "s0_me@t6_ing.ab.cde",
            "s0-m3@7h_1ng.abc.de",
        ];
        
        results = emailAddresses.Select(emailAddress => emailAddress.VerifyEmailAddress()).ToList();
        results.ForEach(result => Assert.That(result, Is.Empty));
    }

    [Test]
    public void Test_VerifyPhoneNumber() {
        // Test for default 9-digit phone numbers
        var phoneNumbers = new[] {
            "123456abc",
            "0123456789",
            "123-45678",
            "123.45678",
            "123 45678",
            "12345678",
        };

        var results = phoneNumbers.Select(phoneNumber => phoneNumber.VerifyPhoneNumber()).ToList();
        var expect = new List<string> { "Phone Number is not in a valid format (eg. 411222333)." };
        results.ForEach(result => Assert.That(result, Is.EquivalentTo(expect)));
        
        // Test for custom 11-digit phone numbers
        phoneNumbers = [
            "123456789ab",
            "061123456789",
            "123-4567890",
            "123.4567890",
            "123 4567890",
            "1234567890",
        ];

        results = phoneNumbers.Select(phoneNumber => phoneNumber.VerifyPhoneNumber(11)).ToList();
        results.ForEach(result => Assert.That(result, Is.EquivalentTo(expect)));
        
        // Test for custom 11-digit phone numbers
        var result = "422333444".VerifyPhoneNumber();
        Assert.That(result, Is.Empty);
        
        result = "1800123456".VerifyPhoneNumber(10);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Test_VerifyUsername() {
        // Test for short and long usernames
        var usernames = new[] {
            "short",
            "sh0rt",
            "sh*rT",
            "very-long-0123456789'.-_!@#*=+:<>~verylong0123456789'.-_!@#*=+:<>~",
        };

        var results = usernames.Select(username => username.VerifyUsername()).ToList();
        var expects = usernames.Select(username => $"Username is too {username.ShortOrLong(6, 65)}. Min 6, max 65 characters.").ToList();
        
        results.ForEach(result => Assert.That(result[0], Is.EquivalentTo(expects[results.IndexOf(result)])));

        // Test for valid username
        var result = "Username0123456789'.-_!@#*=+:<>~".VerifyUsername();
        Assert.That(result, Is.Empty);

        // Test for invalid usernames
        var invalids = new[] {"`", "$", "%", "^", "&", "(", ")", "[", "]", "{", "}", ";", "\"", ",", "/", "?", "\\", "|", " "};
        results = invalids.Select(invalid => $"Username{invalid}".VerifyUsername()).ToList();

        results.ForEach(res => Assert.That(res[0], Is.EquivalentTo("Username should only contain alphabetical letters, numbers, and the following letters: '.-_!@#*=+[]():<>~")));
    }

    [Test]
    public void Test_VerifyName() {
        // Test for short and long names
        var result = "".VerifyName("FirstName");
        Assert.That(result[0], Is.EquivalentTo("First Name is too short. Min 1, max 65 characters."));

        result = "long-name-long-name-long-name-long-name-long-name-long-name-long-name".VerifyName("LastName");
        Assert.That(result[0], Is.EquivalentTo("Last Name is too long. Min 1, max 65 characters."));
        
        // Test for invalid names
        var invalids = new[] {"`", "$", "%", "^", "&", "(", ")", "[", "]", "{", "}", ";", "\"", ",", "/", "?", "\\", "|", "@", "#", "!", "*", "+", "=", ":", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
        var results = invalids.SelectMany(invalid => $"Username{invalid}".VerifyName("Name")).ToList();
        
        results.ForEach(res => Assert.That(res, Is.EquivalentTo("Name should only contain alphabetical letters, dots, hyphens and single quotes.")));
        
        // Test for valid name
        result = "User.name's User_name User-name".VerifyName("Name").ToList();
        Assert.That(result, Is.Empty);
    }
}
