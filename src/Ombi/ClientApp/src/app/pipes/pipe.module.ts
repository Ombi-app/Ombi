import { ModuleWithProviders, NgModule } from "@angular/core";
import { HumanizePipe } from "./HumanizePipe";
import { ThousandShortPipe } from "./ThousandShortPipe";
import { SafePipe } from "./SafePipe";

@NgModule({
    imports:        [],
    declarations:   [HumanizePipe, ThousandShortPipe, SafePipe],
    exports:        [HumanizePipe, ThousandShortPipe, SafePipe],
})
export class PipeModule {

    public static forRoot(): ModuleWithProviders {
        return {
            ngModule: PipeModule,
            providers: [],
        };
    }
}
