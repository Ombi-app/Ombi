import { ApplicationRef, NgModuleRef } from "@angular/core";

/**
 * Legacy HMR bootstrap helper for NgModule-based apps.
 * @angularclass/hmr has been removed; the application builder provides HMR natively.
 */
export const hmrBootstrap = (module: any, bootstrap: () => Promise<NgModuleRef<any>>) => {
  let ngModule: NgModuleRef<any>;
  module.hot.accept();
  bootstrap().then(mod => ngModule = mod);
  module.hot.dispose(() => {
    ngModule.destroy();
  });
};

// HMR for standalone applications using bootstrapApplication
export const hmrBootstrapStandalone = (module: any, bootstrap: () => Promise<ApplicationRef>) => {
  let appRef: ApplicationRef;
  module.hot.accept();
  bootstrap().then(app => appRef = app);
  module.hot.dispose(() => {
    appRef.destroy();
  });
};
