export const AvailableLanguages: ILanguage[] = [
    { display: 'English', value: 'en' },
    { display: 'Français', value: 'fr' },
    { display: 'Dansk', value: 'da' },
    { display: 'Deutsch', value: 'de' },
    { display: 'Italiano', value: 'it' },
    { display: 'Español', value: 'es' },
    { display: 'Nederlands', value: 'nl' },
    { display: 'Norsk', value: 'no' },
    { display: 'Português (Brasil)', value: 'pt' },
    { display: 'Polski', value: 'pl' },
    { display: 'Svenska', value: 'sv' },
];

export interface ILanguage {
    display: string;
    value: string;
}