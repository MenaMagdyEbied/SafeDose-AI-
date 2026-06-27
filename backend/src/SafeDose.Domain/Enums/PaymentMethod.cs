namespace SafeDose.Domain.Enums;

// Which payment instrument the user picked on the checkout page.
// Each one maps to a different Paymob Integration ID.
public enum PaymentMethod : byte
{
    Card = 1,        // Visa, Mastercard, Meeza
    Wallet = 2       // Vodafone Cash, Etisalat Cash, Orange Cash, We Pay
}
