export const AvailableLanguages: ILanguage[] = [
    { display: 'Dansk', value: 'da' },
    { display: 'Deutsch', value: 'de' },
    { display: 'English', value: 'en' },
    { display: 'Español', value: 'es' },
    { display: 'Français', value: 'fr' },
    { display: 'Italiano', value: 'it' },
    { display: 'Magyar', value: 'hu' },
    { display: 'Nederlands', value: 'nl' },
    { display: 'Norsk', value: 'no' },
    { display: 'Polski', value: 'pl' },
    { display: 'Português (Brasil)', value: 'pt' },
    { display: 'Slovenčina', value: 'sk' },
    { display: 'Svenska', value: 'sv' },
    { display: 'Български', value: 'bg' },
    { display: 'Русский', value: 'ru' },
    { display: 'čeština', value: 'cs' },
    { display: '简体中文', value: 'zh' },
];

export interface ILanguage {
    display: string;
    value: string;
}