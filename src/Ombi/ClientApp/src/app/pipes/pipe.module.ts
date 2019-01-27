import { ModuleWithProviders, NgModule } from "@angular/core";
import { HumanizePipe } from "./HumanizePipe";
import { ThousandShortPipe } from "./ThousandShortPipe";

@NgModule({
    imports:        [],
    declarations:   [HumanizePipe, ThousandShortPipe],
    exports:        [HumanizePipe, ThousandShortPipe],
})
export class PipeModule {

    public static forRoot(): ModuleWithProviders {
        return {
            ngModule: PipeModule,
            providers: [],
        };
    }
}
