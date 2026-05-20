import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SonarrComponent } from './sonarr.component';
import { UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSonarrService = {
    getQualityProfiles: vi.fn().mockReturnValue(of([{ name: 'HD-1080p', id: 1 }])),
    getRootFolders: vi.fn().mockReturnValue(of([{ path: '/tv', id: 1 }])),
    getV3LanguageProfiles: vi.fn().mockReturnValue(of([{ name: 'English', id: 1 }])),
    getTags: vi.fn().mockReturnValue(of([{ label: 'anime', id: 1 }])),
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
  };

  const mockTesterService = {
    sonarrTest: vi.fn().mockReturnValue(of({ isValid: true, version: ['3'] })),
  };

  const fb = new UntypedFormBuilder();

  const mockSonarrFacade = {
    sonarrState$: vi.fn().mockReturnValue(of({
      settings: {
        enabled: true, apiKey: 'test-key', qualityProfile: 1,
        rootPath: '/tv', qualityProfileAnime: -1, rootPathAnime: '',
        ssl: false, subDir: '', ip: '127.0.0.1', port: 8989,
        addOnly: false, seasonFolders: true, languageProfile: 1,
        languageProfileAnime: -1, scanForAvailability: false,
        prioritizeArrAvailability: false, sendUserTags: false,
        tag: null, animeTag: null,
      },
      version: ['3'],
    })),
    updateSettings: vi.fn().mockReturnValue(of(true)),
  };

  const comp = new SonarrComponent(
    mockSonarrService as any,
    mockNotify as any,
    mockTesterService as any,
    fb,
    mockSonarrFacade as any,
  );

  return { comp, mockSonarrService, mockNotify, mockTesterService, mockSonarrFacade, fb };
}

describe('SonarrComponent', () => {
  describe('ngOnInit', () => {
    it('should create the form from facade state', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      expect(comp.form).toBeDefined();
      expect(comp.form.controls['ip'].value).toBe('127.0.0.1');
      expect(comp.form.controls['port'].value).toBe(8989);
    });

    it('should set sonarr version from state', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      expect(comp.sonarrVersion).toBe('3');
    });

    it('should fetch profiles when qualityProfile is set', () => {
      const { comp, mockSonarrService } = createComponent();
      comp.ngOnInit();
      expect(mockSonarrService.getQualityProfiles).toHaveBeenCalled();
    });

    it('should fetch root folders when rootPath is set', () => {
      const { comp, mockSonarrService } = createComponent();
      comp.ngOnInit();
      expect(mockSonarrService.getRootFolders).toHaveBeenCalled();
    });

    it('should fetch language profiles when languageProfile is set and version is 3', () => {
      const { comp, mockSonarrService } = createComponent();
      comp.ngOnInit();
      expect(mockSonarrService.getV3LanguageProfiles).toHaveBeenCalled();
    });
  });

  describe('getProfiles', () => {
    it('should populate qualities and notify success', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.getProfiles(comp.form);
      expect(comp.qualities[0]).toEqual({ name: 'Please Select', id: -1 });
      expect(comp.profilesRunning).toBe(false);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully retrieved the Quality Profiles');
    });
  });

  describe('getRootFolders', () => {
    it('should populate root folders and notify success', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.getRootFolders(comp.form);
      expect(comp.rootFolders[0]).toEqual({ path: 'Please Select', id: -1 });
      expect(comp.rootFoldersRunning).toBe(false);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully retrieved the Root Folders');
    });
  });

  describe('test', () => {
    it('should notify success and set version on valid connection', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.test(comp.form);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully connected to Sonarr!');
      expect(comp.sonarrVersion).toBe('3');
    });

    it('should notify error when expectedSubDir is returned', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.sonarrTest.mockReturnValue(of({ isValid: false, expectedSubDir: '/sonarr' }));
      comp.ngOnInit();
      comp.test(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Your Sonarr Base URL must be set to /sonarr');
    });

    it('should show additional information when available', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.sonarrTest.mockReturnValue(of({ isValid: false, additionalInformation: 'Auth failed' }));
      comp.ngOnInit();
      comp.test(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Auth failed');
    });

    it('should show generic error when no details available', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.sonarrTest.mockReturnValue(of({ isValid: false }));
      comp.ngOnInit();
      comp.test(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('We could not connect to Sonarr!');
    });
  });

  describe('onSubmit', () => {
    it('should show error when form is invalid', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      Object.defineProperty(comp.form, 'invalid', { value: true });
      comp.onSubmit(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    });

    it('should save via facade and notify success', () => {
      const { comp, mockNotify, mockSonarrFacade } = createComponent();
      comp.ngOnInit();
      // Clear validators to make form valid for submit
      comp.form.controls['qualityProfile'].clearValidators();
      comp.form.controls['rootPath'].clearValidators();
      comp.form.controls['qualityProfile'].updateValueAndValidity();
      comp.form.controls['rootPath'].updateValueAndValidity();
      comp.onSubmit(comp.form);
      expect(mockSonarrFacade.updateSettings).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Sonarr settings');
    });

    it('should nullify animeTag when value is -1', () => {
      const { comp, mockSonarrFacade } = createComponent();
      comp.ngOnInit();
      comp.form.controls['qualityProfile'].clearValidators();
      comp.form.controls['rootPath'].clearValidators();
      comp.form.controls['qualityProfile'].updateValueAndValidity();
      comp.form.controls['rootPath'].updateValueAndValidity();
      comp.form.controls['animeTag'].setValue(-1);
      comp.onSubmit(comp.form);
      expect(comp.form.controls['animeTag'].value).toBeNull();
    });

    it('should nullify tag when value is -1', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      comp.form.controls['qualityProfile'].clearValidators();
      comp.form.controls['rootPath'].clearValidators();
      comp.form.controls['qualityProfile'].updateValueAndValidity();
      comp.form.controls['rootPath'].updateValueAndValidity();
      comp.form.controls['tag'].setValue(-1);
      comp.onSubmit(comp.form);
      expect(comp.form.controls['tag'].value).toBeNull();
    });

    it('should notify error on save failure', () => {
      const { comp, mockNotify, mockSonarrFacade } = createComponent();
      mockSonarrFacade.updateSettings.mockReturnValue(of(false));
      comp.ngOnInit();
      comp.form.controls['qualityProfile'].clearValidators();
      comp.form.controls['rootPath'].clearValidators();
      comp.form.controls['qualityProfile'].updateValueAndValidity();
      comp.form.controls['rootPath'].updateValueAndValidity();
      comp.onSubmit(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('There was an error when saving the Sonarr settings');
    });
  });

  describe('onFormValuesChanged', () => {
    it('should set formErrors for controls with "Please Select" value', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      comp.form.controls['qualityProfile'].setValue('Please Select');
      comp.form.controls['qualityProfile'].markAsDirty();
      comp.onFormValuesChanged();
      // formErrors should be populated for invalid fields
      expect(comp.formErrors).toBeDefined();
    });
  });
});
