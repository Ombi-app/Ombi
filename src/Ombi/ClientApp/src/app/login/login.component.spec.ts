import { describe, it, expect, beforeEach, vi } from 'vitest';
import { LoginComponent } from './login.component';
import { of } from 'rxjs';
import { UntypedFormBuilder, Validators } from '@angular/forms';

function createComponent() {
  const mockAuth = {
    loggedIn: vi.fn().mockReturnValue(false),
    login: vi.fn().mockReturnValue(of({ access_token: 'test-token' })),
    requiresPassword: vi.fn().mockReturnValue(of(false)),
    oAuth: vi.fn().mockReturnValue(of({})),
    headerAuth: vi.fn().mockReturnValue(of({ access_token: 'header-token' })),
  };
  const mockRouter = {
    navigate: vi.fn(),
  };
  const mockStatus = {
    getWizardStatus: vi.fn().mockReturnValue(of({ result: true })),
  };
  const fb = new UntypedFormBuilder();
  const mockSettingsService = {
    getLandingPage: vi.fn().mockReturnValue(of({ enabled: false })),
    getAuthentication: vi.fn().mockReturnValue(of({ allowNoPassword: false, enableHeaderAuth: false })),
    getClientId: vi.fn().mockReturnValue(of('test-client-id')),
  };
  const mockCustomizationFacade = {
    settings$: vi.fn().mockReturnValue(of({ applicationName: 'TestOmbi' })),
  };
  const mockRoute = {
    params: of({}),
  };
  const mockTranslate = {
    get: vi.fn().mockReturnValue(of('translated')),
    instant: vi.fn((key: string) => key),
  };
  const mockPlexTv = {
    GetPin: vi.fn().mockReturnValue(of({ id: 1, code: 'abc' })),
  };
  const mockStore = {
    save: vi.fn(),
    get: vi.fn(),
    remove: vi.fn(),
  };
  const mockSonarrFacade = {
    load: vi.fn().mockReturnValue(of({})),
  };
  const mockRadarrFacade = {
    load: vi.fn().mockReturnValue(of({})),
  };
  const mockNotify = {
    open: vi.fn(),
  };

  const comp = new LoginComponent(
    mockAuth as any,
    mockRouter as any,
    mockStatus as any,
    fb,
    mockSettingsService as any,
    mockCustomizationFacade as any,
    mockRoute as any,
    '/',
    mockTranslate as any,
    mockPlexTv as any,
    mockStore as any,
    mockSonarrFacade as any,
    mockRadarrFacade as any,
    mockNotify as any,
  );

  return {
    comp, mockAuth, mockRouter, mockStore, mockSettingsService,
    mockNotify, mockSonarrFacade, mockRadarrFacade, mockCustomizationFacade,
  };
}

describe('LoginComponent', () => {
  describe('constructor', () => {
    it('should create the form with username, password, and rememberMe', () => {
      const { comp } = createComponent();
      expect(comp.form).toBeDefined();
      expect(comp.form.get('username')).toBeDefined();
      expect(comp.form.get('password')).toBeDefined();
      expect(comp.form.get('rememberMe')).toBeDefined();
    });

    it('should redirect to wizard if wizard not complete', () => {
      const mockAuth = {
        loggedIn: vi.fn().mockReturnValue(false),
      };
      const mockRouter = { navigate: vi.fn() };
      const mockStatus = {
        getWizardStatus: vi.fn().mockReturnValue(of({ result: false })),
      };
      const fb = new UntypedFormBuilder();

      new LoginComponent(
        mockAuth as any, mockRouter as any, mockStatus as any, fb,
        { getLandingPage: vi.fn().mockReturnValue(of({ enabled: false })),
          getAuthentication: vi.fn().mockReturnValue(of({})),
          getClientId: vi.fn().mockReturnValue(of('')) } as any,
        { settings$: vi.fn().mockReturnValue(of({})) } as any,
        { params: of({}) } as any, '/',
        { get: vi.fn().mockReturnValue(of('')) } as any,
        {} as any,
        { save: vi.fn() } as any,
        { load: vi.fn().mockReturnValue(of({})) } as any,
        { load: vi.fn().mockReturnValue(of({})) } as any,
        { open: vi.fn() } as any,
      );

      expect(mockRouter.navigate).toHaveBeenCalledWith(['Wizard']);
    });

    it('should redirect to home when already logged in', () => {
      const mockAuth = {
        loggedIn: vi.fn().mockReturnValue(true),
      };
      const mockRouter = { navigate: vi.fn() };
      const mockSonarrFacade = { load: vi.fn().mockReturnValue(of({})) };
      const mockRadarrFacade = { load: vi.fn().mockReturnValue(of({})) };

      new LoginComponent(
        mockAuth as any, mockRouter as any,
        { getWizardStatus: vi.fn().mockReturnValue(of({ result: true })) } as any,
        new UntypedFormBuilder(),
        { getLandingPage: vi.fn().mockReturnValue(of({ enabled: false })),
          getAuthentication: vi.fn().mockReturnValue(of({})),
          getClientId: vi.fn().mockReturnValue(of('')) } as any,
        { settings$: vi.fn().mockReturnValue(of({})) } as any,
        { params: of({}) } as any, '/',
        { get: vi.fn().mockReturnValue(of('')) } as any,
        {} as any,
        { save: vi.fn() } as any,
        mockSonarrFacade as any,
        mockRadarrFacade as any,
        { open: vi.fn() } as any,
      );

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
      expect(mockSonarrFacade.load).toHaveBeenCalled();
      expect(mockRadarrFacade.load).toHaveBeenCalled();
    });
  });

  describe('appName', () => {
    it('should return application name from settings', () => {
      const { comp } = createComponent();
      comp.customizationSettings = { applicationName: 'MyOmbi' } as any;
      expect(comp.appName).toBe('MyOmbi');
    });

    it('should return Ombi when no custom name set', () => {
      const { comp } = createComponent();
      comp.customizationSettings = { applicationName: '' } as any;
      expect(comp.appName).toBe('Ombi');
    });

    it('should return Ombi when applicationName is undefined', () => {
      const { comp } = createComponent();
      comp.customizationSettings = {} as any;
      expect(comp.appName).toBe('Ombi');
    });
  });

  describe('appNameTranslate', () => {
    it('should return object with appName key', () => {
      const { comp } = createComponent();
      comp.customizationSettings = { applicationName: 'TestApp' } as any;
      expect(comp.appNameTranslate).toEqual({ appName: 'TestApp' });
    });
  });

  describe('onSubmit', () => {
    it('should show validation error for invalid form', () => {
      const { comp, mockNotify } = createComponent();
      // Form is invalid because username is required and empty
      comp.onSubmit(comp.form);
      expect(mockNotify.open).toHaveBeenCalled();
    });

    it('should call authService.requiresPassword for valid form', () => {
      const { comp, mockAuth } = createComponent();
      comp.form.patchValue({ username: 'testuser', password: 'pass123' });
      comp.authenticationSettings = { allowNoPassword: false } as any;

      comp.onSubmit(comp.form);

      expect(mockAuth.requiresPassword).toHaveBeenCalled();
    });

    it('should save token and navigate on successful login', () => {
      const { comp, mockAuth, mockStore, mockRouter } = createComponent();
      comp.form.patchValue({ username: 'testuser', password: 'pass123' });
      comp.authenticationSettings = { allowNoPassword: false } as any;

      mockAuth.requiresPassword.mockReturnValue(of(false));
      mockAuth.login.mockReturnValue(of({ access_token: 'new-token' }));
      mockAuth.loggedIn.mockReturnValue(true);

      comp.onSubmit(comp.form);

      expect(mockStore.save).toHaveBeenCalledWith('id_token', 'new-token');
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
    });

    it('should show error when login fails (no valid token)', () => {
      const { comp, mockAuth, mockNotify } = createComponent();
      comp.form.patchValue({ username: 'testuser', password: 'wrong' });
      comp.authenticationSettings = { allowNoPassword: false } as any;

      mockAuth.requiresPassword.mockReturnValue(of(false));
      mockAuth.login.mockReturnValue(of({ access_token: 'bad-token' }));
      mockAuth.loggedIn.mockReturnValue(false);

      comp.onSubmit(comp.form);

      expect(mockNotify.open).toHaveBeenCalled();
    });
  });

  describe('headerAuth', () => {
    it('should not call headerAuth when not enabled', () => {
      const { comp, mockAuth } = createComponent();
      comp.authenticationSettings = { enableHeaderAuth: false } as any;
      comp.headerAuth();
      expect(mockAuth.headerAuth).not.toHaveBeenCalled();
    });

    it('should save token when header auth succeeds', () => {
      const { comp, mockAuth, mockStore } = createComponent();
      comp.authenticationSettings = { enableHeaderAuth: true } as any;
      mockAuth.headerAuth.mockReturnValue(of({ access_token: 'header-token' }));
      mockAuth.loggedIn.mockReturnValue(true);

      comp.headerAuth();

      expect(mockStore.save).toHaveBeenCalledWith('id_token', 'header-token');
    });
  });

  describe('ngOnInit', () => {
    it('should subscribe to customization settings', () => {
      const { comp, mockCustomizationFacade } = createComponent();
      comp.ngOnInit();
      expect(mockCustomizationFacade.settings$).toHaveBeenCalled();
    });

    it('should set baseUrl when href length > 1', () => {
      // Create component with non-root base href
      const mockAuth = { loggedIn: vi.fn().mockReturnValue(false) };
      const fb = new UntypedFormBuilder();
      const comp2 = new LoginComponent(
        mockAuth as any,
        { navigate: vi.fn() } as any,
        { getWizardStatus: vi.fn().mockReturnValue(of({ result: true })) } as any,
        fb,
        {
          getLandingPage: vi.fn().mockReturnValue(of({ enabled: false })),
          getAuthentication: vi.fn().mockReturnValue(of({ enableHeaderAuth: false })),
          getClientId: vi.fn().mockReturnValue(of(''))
        } as any,
        { settings$: vi.fn().mockReturnValue(of({})) } as any,
        { params: of({}) } as any,
        '/ombi',
        { get: vi.fn().mockReturnValue(of('')) } as any,
        {} as any,
        { save: vi.fn() } as any,
        { load: vi.fn().mockReturnValue(of({})) } as any,
        { load: vi.fn().mockReturnValue(of({})) } as any,
        { open: vi.fn() } as any,
      );

      comp2.ngOnInit();
      expect(comp2.baseUrl).toBe('/ombi');
    });
  });

  describe('ngOnDestroy', () => {
    it('should clear pin timer', () => {
      const { comp } = createComponent();
      const clearIntervalSpy = vi.spyOn(globalThis, 'clearInterval');
      comp.pinTimer = setInterval(() => {}, 10000);
      const timerId = comp.pinTimer;
      comp.ngOnDestroy();
      expect(clearIntervalSpy).toHaveBeenCalledWith(timerId);
      clearIntervalSpy.mockRestore();
    });
  });
});
