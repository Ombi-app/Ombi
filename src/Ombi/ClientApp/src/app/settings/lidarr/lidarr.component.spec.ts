import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LidarrComponent } from './lidarr.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getLidarr: vi.fn().mockReturnValue(of({
      enabled: true, apiKey: 'test-key', defaultQualityProfile: 1,
      defaultRootPath: '/music', ssl: false, subDir: '',
      ip: '127.0.0.1', port: 8686, albumFolder: true,
      metadataProfileId: 1, addOnly: false,
    })),
    saveLidarr: vi.fn().mockReturnValue(of(true)),
  };

  const mockLidarrService = {
    getQualityProfiles: vi.fn().mockReturnValue(of([{ name: 'Lossless', id: 1 }])),
    getRootFolders: vi.fn().mockReturnValue(of([{ path: '/music', id: 1 }])),
    getMetadataProfiles: vi.fn().mockReturnValue(of([{ name: 'Standard', id: 1 }])),
  };

  const mockNotify = {
    error: vi.fn(),
    success: vi.fn(),
  };

  const fb = new UntypedFormBuilder();

  const mockTesterService = {
    lidarrTest: vi.fn().mockReturnValue(of({ isValid: true })),
  };

  const comp = new LidarrComponent(
    mockSettingsService as any,
    mockLidarrService as any,
    mockNotify as any,
    fb,
    mockTesterService as any,
  );

  return { comp, mockSettingsService, mockLidarrService, mockNotify, mockTesterService, fb };
}

describe('LidarrComponent', () => {
  describe('ngOnInit', () => {
    it('should create the form from settings', () => {
      const { comp } = createComponent();
      comp.ngOnInit();
      expect(comp.form).toBeDefined();
      expect(comp.form.controls['ip'].value).toBe('127.0.0.1');
      expect(comp.form.controls['port'].value).toBe(8686);
    });

    it('should fetch profiles when quality profile is set', () => {
      const { comp, mockLidarrService } = createComponent();
      comp.ngOnInit();
      expect(mockLidarrService.getQualityProfiles).toHaveBeenCalled();
    });

    it('should fetch root folders when root path is set', () => {
      const { comp, mockLidarrService } = createComponent();
      comp.ngOnInit();
      expect(mockLidarrService.getRootFolders).toHaveBeenCalled();
    });

    it('should fetch metadata profiles when metadataProfileId is set', () => {
      const { comp, mockLidarrService } = createComponent();
      comp.ngOnInit();
      expect(mockLidarrService.getMetadataProfiles).toHaveBeenCalled();
    });
  });

  describe('getProfiles', () => {
    it('should populate qualities with "Please Select" prepended', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.getProfiles(comp.form);
      expect(comp.qualities[0]).toEqual({ name: 'Please Select', id: -1 });
      expect(comp.profilesRunning).toBe(false);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully retrieved the Quality Profiles');
    });
  });

  describe('getRootFolders', () => {
    it('should populate root folders with "Please Select" prepended', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.getRootFolders(comp.form);
      expect(comp.rootFolders[0]).toEqual({ path: 'Please Select', id: -1 });
      expect(comp.rootFoldersRunning).toBe(false);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully retrieved the Root Folders');
    });
  });

  describe('getMetadataProfiles', () => {
    it('should populate metadata profiles with "Please Select" prepended', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.getMetadataProfiles(comp.form);
      expect(comp.metadataProfiles[0]).toEqual({ name: 'Please Select', id: -1 });
      expect(comp.metadataRunning).toBe(false);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully retrieved the Metadata profiles');
    });
  });

  describe('test', () => {
    it('should notify success on valid connection', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.test(comp.form);
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully connected to Lidarr!');
    });

    it('should notify error with expectedSubDir', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.lidarrTest.mockReturnValue(of({ isValid: false, expectedSubDir: '/lidarr' }));
      comp.ngOnInit();
      comp.test(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Your Lidarr Base URL must be set to /lidarr');
    });

    it('should notify generic error on connection failure', () => {
      const { comp, mockNotify, mockTesterService } = createComponent();
      mockTesterService.lidarrTest.mockReturnValue(of({ isValid: false }));
      comp.ngOnInit();
      comp.test(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('We could not connect to Lidarr!');
    });

    it('should show validation error when form is invalid', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.form.controls['apiKey'].setValue('');
      comp.test(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    });
  });

  describe('onSubmit', () => {
    it('should save and notify success', () => {
      const { comp, mockSettingsService, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.onSubmit(comp.form);
      expect(mockSettingsService.saveLidarr).toHaveBeenCalled();
      expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved Lidarr settings');
    });

    it('should show error when form is invalid', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.form.controls['apiKey'].setValue('');
      comp.onSubmit(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    });

    it('should show error when quality profile is -1', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.form.controls['defaultQualityProfile'].setValue('-1');
      comp.onSubmit(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    });

    it('should show error when root path is "Please Select"', () => {
      const { comp, mockNotify } = createComponent();
      comp.ngOnInit();
      comp.form.controls['defaultRootPath'].setValue('Please Select');
      comp.onSubmit(comp.form);
      expect(mockNotify.error).toHaveBeenCalledWith('Please check your entered values');
    });

    it('should notify on save failure', () => {
      const { comp, mockNotify, mockSettingsService } = createComponent();
      mockSettingsService.saveLidarr.mockReturnValue(of(false));
      comp.ngOnInit();
      comp.onSubmit(comp.form);
      expect(mockNotify.success).toHaveBeenCalledWith('There was an error when saving the Lidarr settings');
    });
  });
});
