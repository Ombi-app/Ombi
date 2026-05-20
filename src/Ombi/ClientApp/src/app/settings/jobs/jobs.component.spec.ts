import { describe, it, expect, vi } from 'vitest';
import { JobsComponent } from './jobs.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockSettingsService = {
    getJobSettings: vi.fn().mockReturnValue(of({
      automaticUpdater: '0 0 * * *', couchPotatoSync: '0 0 * * *',
      embyContentSync: '0 0 * * *', jellyfinContentSync: '0 0 * * *',
      plexContentSync: '0 0 * * *', userImporter: '0 0 * * *',
      sonarrSync: '0 0 * * *', radarrSync: '0 0 * * *',
      sickRageSync: '0 0 * * *', newsletter: '0 0 * * *',
      plexRecentlyAddedSync: '0 0 * * *', lidarrArtistSync: '0 0 * * *',
      issuesPurge: '0 0 * * *', retryRequests: '0 0 * * *',
      mediaDatabaseRefresh: '0 0 * * *', autoDeleteRequests: '0 0 * * *',
      embyRecentlyAddedSync: '0 0 * * *', plexWatchlistImport: '0 0 * * *',
    })),
    saveJobSettings: vi.fn().mockReturnValue(of({ result: true })),
    testCron: vi.fn().mockReturnValue(of({ success: true })),
  };
  const fb = new UntypedFormBuilder();
  const mockNotify = { error: vi.fn(), success: vi.fn() };
  const mockJobService = { runArrAvailabilityChecker: vi.fn().mockReturnValue(of(true)) };

  const comp = new JobsComponent(mockSettingsService as any, fb, mockNotify as any, mockJobService as any);
  return { comp, mockSettingsService, mockNotify, mockJobService };
}

describe('JobsComponent', () => {
  it('should load job settings and build form', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.form).toBeDefined();
    expect(comp.form.controls['automaticUpdater'].value).toBe('0 0 * * *');
  });

  it('should save and notify success', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the job settings');
  });

  it('should notify error on save failure', () => {
    const { comp, mockNotify, mockSettingsService } = createComponent();
    mockSettingsService.saveJobSettings.mockReturnValue(of({ result: false, message: 'Invalid cron' }));
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('There was an error when saving the job settings. Invalid cron');
  });

  it('should validate cron expression as valid', () => {
    const { comp, mockNotify } = createComponent();
    comp.testCron('0 0 * * *');
    expect(mockNotify.success).toHaveBeenCalledWith('Cron is Valid');
  });

  it('should show error for invalid cron expression', () => {
    const { comp, mockNotify, mockSettingsService } = createComponent();
    mockSettingsService.testCron.mockReturnValue(of({ success: false, message: 'Invalid expression' }));
    comp.testCron('bad-cron');
    expect(mockNotify.error).toHaveBeenCalledWith('Invalid expression');
  });

  it('should run arr availability checker', () => {
    const { comp, mockJobService } = createComponent();
    comp.runArrAvailabilityChecker();
    expect(mockJobService.runArrAvailabilityChecker).toHaveBeenCalled();
  });
});
