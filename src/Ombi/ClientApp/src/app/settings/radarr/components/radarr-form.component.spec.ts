import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RadarrFormComponent } from './radarr-form.component';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { of } from 'rxjs';

function createComponent(formOverrides: Record<string, any> = {}) {
  const fb = new UntypedFormBuilder();
  const defaults = {
    enabled: false, apiKey: '', defaultQualityProfile: -1,
    defaultRootPath: '', tag: null, sendUserTags: false,
    ssl: false, subDir: '', ip: '', port: 7878,
    addOnly: false, minimumAvailability: 'Announced',
    scanForAvailability: false, prioritizeArrAvailability: false,
  };
  const form = fb.group({ ...defaults, ...formOverrides });

  const mockRadarrService = {
    getQualityProfiles: vi.fn().mockReturnValue(of([{ name: 'HD', id: 1 }])),
    getRootFolders: vi.fn().mockReturnValue(of([{ path: '/movies', id: 1 }])),
    getTagsWithSettings: vi.fn().mockReturnValue(of([{ label: 'test', id: 1 }])),
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
  };

  const mockTesterService = {
    radarrTest: vi.fn().mockReturnValue(of({ isValid: true })),
  };

  const mockControlContainer = {
    control: form,
  };

  const comp = new RadarrFormComponent(
    mockRadarrService as any,
    mockNotify as any,
    mockTesterService as any,
    mockControlContainer as any,
  );

  return { comp, form, mockRadarrService, mockNotify, mockTesterService };
}

describe('RadarrFormComponent', () => {
  describe('ngOnInit', () => {
    it('should initialize with default quality and root folder options', () => {
      const { comp } = createComponent();
      comp.ngOnInit();

      expect(comp.qualities).toContainEqual({ name: 'Please Select', id: -1 });
      expect(comp.rootFolders).toContainEqual({ path: 'Please Select', id: -1 });
      expect(comp.tags).toContainEqual({ label: 'None', id: -1 });
    });

    it('should set minimumAvailabilityOptions', () => {
      const { comp } = createComponent();
      comp.ngOnInit();

      expect(comp.minimumAvailabilityOptions).toHaveLength(3);
      expect(comp.minimumAvailabilityOptions[0].name).toBe('Announced');
      expect(comp.minimumAvailabilityOptions[1].value).toBe('InCinemas');
      expect(comp.minimumAvailabilityOptions[2].value).toBe('Released');
    });

    it('should fetch profiles when defaultQualityProfile has value', () => {
      const { comp, mockRadarrService } = createComponent({ defaultQualityProfile: 1 });
      comp.ngOnInit();
      expect(mockRadarrService.getQualityProfiles).toHaveBeenCalled();
    });

    it('should fetch root folders when defaultRootPath has value', () => {
      const { comp, mockRadarrService } = createComponent({ defaultRootPath: '/movies' });
      comp.ngOnInit();
      expect(mockRadarrService.getRootFolders).toHaveBeenCalled();
    });

    it('should fetch tags when tag has value', () => {
      const { comp, mockRadarrService } = createComponent({ tag: 1 });
      comp.ngOnInit();
      expect(mockRadarrService.getTagsWithSettings).toHaveBeenCalled();
    });
  });

  describe('toggleValidators', () => {
    it('should set validators when enabled', () => {
      const { comp } = createComponent({ enabled: true });
      comp.ngOnInit();
      comp.toggleValidators();

      expect(comp.form.controls['apiKey'].validator).not.toBeNull();
      expect(comp.form.controls['ip'].validator).not.toBeNull();
      expect(comp.form.controls['port'].validator).not.toBeNull();
    });

    it('should clear validators when disabled', () => {
      const { comp } = createComponent({ enabled: false });
      comp.ngOnInit();
      comp.toggleValidators();

      expect(comp.form.controls['apiKey'].validator).toBeNull();
      expect(comp.form.controls['ip'].validator).toBeNull();
    });
  });

  describe('getProfiles', () => {
    it('should fetch and prepend "Please Select" option', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.getProfiles(comp.form);

      expect(comp.qualities[0]).toEqual({ name: 'Please Select', id: -1 });
      expect(comp.qualities.length).toBeGreaterThanOrEqual(2);
      expect(comp.profilesRunning).toBe(false);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully retrieved the Quality Profiles');
    });
  });

  describe('getRootFolders', () => {
    it('should fetch and prepend "Please Select" option', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.getRootFolders(comp.form);

      expect(comp.rootFolders[0]).toEqual({ path: 'Please Select', id: -1 });
      expect(comp.rootFolders.length).toBeGreaterThanOrEqual(2);
      expect(comp.rootFoldersRunning).toBe(false);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully retrieved the Root Folders');
    });
  });

  describe('test', () => {
    it('should notify success when connection is valid', () => {
      const { comp, mockNotify, mockTesterService } = createComponent({
        enabled: true, apiKey: 'key', ip: '127.0.0.1', port: 7878,
        defaultQualityProfile: 1, defaultRootPath: '/movies',
        minimumAvailability: 'Announced',
      });
      comp.ngOnInit();
      comp.toggleValidators();
      comp.test(comp.form);

      expect(mockNotify.success).toHaveBeenCalledWith('Successfully connected to Radarr!');
    });

    it('should notify error and set subDir when expectedSubDir is returned', () => {
      const { comp, mockNotify, mockTesterService } = createComponent({
        enabled: true, apiKey: 'key', ip: '127.0.0.1', port: 7878,
        defaultQualityProfile: 1, defaultRootPath: '/movies',
        minimumAvailability: 'Announced',
      });
      mockTesterService.radarrTest.mockReturnValue(of({ isValid: false, expectedSubDir: '/radarr' }));
      comp.ngOnInit();
      comp.toggleValidators();
      comp.test(comp.form);

      expect(mockNotify.error).toHaveBeenCalledWith('Your Radarr Base URL must be set to /radarr');
      expect(comp.form.controls['subDir'].value).toBe('/radarr');
    });

    it('should notify generic error when connection fails', () => {
      const { comp, mockNotify, mockTesterService } = createComponent({
        enabled: true, apiKey: 'key', ip: '127.0.0.1', port: 7878,
        defaultQualityProfile: 1, defaultRootPath: '/movies',
        minimumAvailability: 'Announced',
      });
      mockTesterService.radarrTest.mockReturnValue(of({ isValid: false }));
      comp.ngOnInit();
      comp.toggleValidators();
      comp.test(comp.form);

      expect(mockNotify.error).toHaveBeenCalledWith('We could not connect to Radarr!');
    });

    it('should show error when form is invalid', () => {
      const { comp, mockNotify } = createComponent({ enabled: true });
      comp.ngOnInit();
      comp.toggleValidators();
      // Force form controls to update validity
      Object.keys(comp.form.controls).forEach(key => {
        comp.form.controls[key].updateValueAndValidity();
      });
      comp.form.updateValueAndValidity();

      comp.test(comp.form);

      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    });
  });
});
