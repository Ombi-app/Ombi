import { ApplicationRef, NgModuleRef } from "@angular/core";
import { createNewHosts } from "@angularclass/hmr";

export const hmrBootstrap = (module: any, bootstrap: () => Promise<NgModuleRef<any>>) => {
  let ngModule: NgModuleRef<any>;
  module.hot.accept();
  bootstrap().then(mod => ngModule = mod);
  module.hot.dispose(() => {
    const appRef: ApplicationRef = ngModule.injector.get(ApplicationRef);
    const elements = appRef.components.map(c => c.location.nativeElement);
    const makeVisible = createNewHosts(elements);
    ngModule.destroy();
    makeVisible();
  });
};

// HMR for standalone applications using bootstrapApplication
export const hmrBootstrapStandalone = (module: any, bootstrap: () => Promise<ApplicationRef>) => {
  let appRef: ApplicationRef;
  module.hot.accept();
  bootstrap().then(app => appRef = app);
  module.hot.dispose(() => {
    const elements = appRef.components.map(c => c.location.nativeElement);
    const makeVisible = createNewHosts(elements);
    appRef.destroy();
    makeVisible();
  });
};
