import { ModuleWithProviders, NgModule } from "@angular/core";
import { HumanizePipe } from "./HumanizePipe";

@NgModule({
    imports:        [],
    declarations:   [HumanizePipe],
    exports:        [HumanizePipe],
})
export class PipeModule {

    public static forRoot(): ModuleWithProviders {
        return {
            ngModule: PipeModule,
            providers: [],
        };
    }
}
