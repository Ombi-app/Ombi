import { ISettings } from "./ICommon";

export interface IExternalSettings extends ISettings {
  ssl: boolean;
  subDir: string;
  ip: string;
  port: number;
}

export interface IOmbiSettings extends ISettings {
  baseUrl: string;
  collectAnalyticData: boolean;
  wizard: boolean;
  apiKey: string;
  ignoreCertificateErrors: boolean;
}

export interface IUpdateSettings extends ISettings {
  autoUpdateEnabled: boolean;
  username: string;
  password: string;
  processName: string;
  useScript: boolean;
  scriptLocation: string;
}

export interface IEmbySettings extends ISettings {
  enable: boolean;
  servers: IEmbyServer[];
}

export interface IEmbyServer extends IExternalSettings {
  name: string;
  apiKey: string;
  administratorId: string;
  enableEpisodeSearching: boolean;
}

export interface IPlexSettings extends ISettings {
  enable: boolean;
  servers: IPlexServer[];
}

export interface IPlexServer extends IExternalSettings {
  name: string;
  enableEpisodeSearching: boolean;
  plexAuthToken: string;
  machineIdentifier: string;
  episodeBatchSize: number;
  plexSelectedLibraries: IPlexLibrariesSettings[];
}

export interface IPlexLibrariesSettings {
  key: string;
  title: string;
  enabled: boolean;
}

export interface ISonarrSettings extends IExternalSettings {
  apiKey: string;
  enabled: boolean;
  qualityProfile: string;
  seasonFolders: boolean;
  rootPath: string;
  fullRootPath: string;
  addOnly: boolean;
}

export interface IRadarrSettings extends IExternalSettings {
  enabled: boolean;
  apiKey: string;
  defaultQualityProfile: string;
  defaultRootPath: string;
  fullRootPath: string;
  addOnly: boolean;
  minimumAvailability: string;
}

export interface ILandingPageSettings extends ISettings {
  enabled: boolean;

  noticeEnabled: boolean;
  noticeText: string;

  timeLimit: boolean;
  startDateTime: Date;
  endDateTime: Date;
  expired: boolean;
}

export interface ICustomizationSettings extends ISettings {
  applicationName: string;
  applicationUrl: string;
  logo: string;
  customCssLink: string;
  hasPresetTheme: boolean;
  presetThemeName: string;
  presetThemeContent: string;
  presetThemeDisplayName: string;
  presetThemeVersion: string;
}

export interface IThemes {
  fullName: string;
  displayName: string;
  version: string;
  url: string;
}

export interface IJobSettings {
  embyContentSync: string;
  sonarrSync: string;
  radarrSync: string;
  plexContentSync: string;
  couchPotatoSync: string;
  automaticUpdater: string;
  userImporter: string;
  sickRageSync: string;
}

export interface IAuthenticationSettings extends ISettings {
  allowExternalUsersToAuthenticate: boolean;
  // Password

  requiredDigit: boolean;
  requiredLength: number;
  requiredLowercase: boolean;
  requireNonAlphanumeric: boolean;
  requireUppercase: boolean;
}

export interface IUserManagementSettings extends ISettings {
  importPlexUsers: boolean;
  importPlexAdmin: boolean;
  importEmbyUsers: boolean;
  defaultRoles: string[];
  bannedPlexUserIds: string[];
  bannedEmbyUserIds: string[];
}

export interface IAbout {
  version: string;
  branch: string;
  osArchitecture: string;
  osDescription: string;
  processArchitecture: string;
  applicationBasePath: string;
}

export interface ICouchPotatoSettings extends IExternalSettings {
  enabled: boolean;
  apiKey: string;
  defaultProfileId: string;
  username: string;
  password: string;
}

export interface ISickRageSettings extends IExternalSettings {
  enabled: boolean;
  apiKey: string;
  qualityProfile: string;
  qualities: IDropDownModel[];
}

export interface IDropDownModel {
  value: string;
  display: string;
}

export interface IDogNzbSettings extends ISettings {
  enabled: boolean;
  apiKey: string;
  movies: boolean;
  tvShows: boolean;
}
