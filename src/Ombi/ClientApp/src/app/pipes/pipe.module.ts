import { ModuleWithProviders, NgModule } from "@angular/core";
import { HumanizePipe } from "./HumanizePipe";
import { ThousandShortPipe } from "./ThousandShortPipe";
import { SafePipe } from "./SafePipe";
import { QualityPipe } from "./QualityPipe";

@NgModule({
    imports:        [],
    declarations:   [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe],
    exports:        [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe],
})
export class PipeModule {

    public static forRoot(): ModuleWithProviders<PipeModule> {
        return {
            ngModule: PipeModule,
            providers: [],
        };
    }
}
