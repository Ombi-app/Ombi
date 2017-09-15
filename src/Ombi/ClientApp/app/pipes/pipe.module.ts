import { NgModule, ModuleWithProviders }      from '@angular/core';
import { HumanizePipe } from './HumanizePipe';

@NgModule({
    imports:        [],
    declarations:   [HumanizePipe],
    exports:        [HumanizePipe],
})
export class PipeModule {

    static forRoot() : ModuleWithProviders {
        return {
            ngModule: PipeModule,
            providers: [],
        };
    }
} 