import { describe, it, expect, vi } from 'vitest';
import { AboutComponent } from './about.component';
import { of } from 'rxjs';

function createComponent(baseHref = '/') {
  const mockSettingsService = {
    about: vi.fn().mockReturnValue(of({
      version: '4.58.0', branch: 'main', frameworkDescription: '.NET 8',
      osArchitecture: 'X64', osDescription: 'Linux',
      processArchitecture: 'X64', applicationBasePath: '/opt/ombi',
      ombiDatabaseType: 'Sqlite', externalDatabaseType: 'Sqlite',
      settingsDatabaseType: 'Sqlite',
    })),
  };
  const mockUpdateService = {
    checkForUpdate: vi.fn().mockReturnValue(of({ updateAvailable: false })),
  };
  const mockHubService = {
    getConnectedUsers: vi.fn().mockResolvedValue([{ userId: '1', displayName: 'Admin' }]),
  };
  const mockDialog = { open: vi.fn() };

  const comp = new AboutComponent(
    mockSettingsService as any, mockUpdateService as any,
    mockHubService as any, mockDialog as any, baseHref,
  );

  return { comp, mockSettingsService, mockUpdateService, mockHubService, mockDialog };
}

describe('AboutComponent', () => {
  it('should load about info and check for updates', async () => {
    const { comp, mockSettingsService, mockUpdateService } = createComponent();
    await comp.ngOnInit();
    expect(mockSettingsService.about).toHaveBeenCalled();
    expect(mockUpdateService.checkForUpdate).toHaveBeenCalled();
    expect(comp.about).toBeDefined();
  });

  it('should set newUpdate when update is available', async () => {
    const { comp, mockUpdateService } = createComponent();
    mockUpdateService.checkForUpdate.mockReturnValue(of({ updateAvailable: true, updateVersionString: '4.59.0' }));
    await comp.ngOnInit();
    expect(comp.newUpdate).toBe(true);
  });

  it('should load connected users', async () => {
    const { comp } = createComponent();
    await comp.ngOnInit();
    expect(comp.connectedUsers).toHaveLength(1);
  });

  it('should set appstore image with base href', async () => {
    const { comp } = createComponent('/ombi');
    await comp.ngOnInit();
    expect(comp.appstoreImage).toBe('../../../ombi/images/appstore.svg');
  });

  describe('usingSqliteDatabase', () => {
    it('should return true when any database is sqlite', async () => {
      const { comp } = createComponent();
      await comp.ngOnInit();
      expect(comp.usingSqliteDatabase).toBe(true);
    });
  });

  it('should open update dialog', async () => {
    const { comp, mockDialog, mockUpdateService } = createComponent();
    mockUpdateService.checkForUpdate.mockReturnValue(of({ updateAvailable: true }));
    await comp.ngOnInit();
    comp.openUpdate();
    expect(mockDialog.open).toHaveBeenCalled();
  });
});
