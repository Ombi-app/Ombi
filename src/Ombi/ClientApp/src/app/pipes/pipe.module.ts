import { ModuleWithProviders, NgModule } from "@angular/core";
import { HumanizePipe } from "./HumanizePipe";
import { LocalizedDatePipe } from "./LocalizedDate";
import { TranslateStatusPipe } from "./TranslateStatus";
import { ThousandShortPipe } from "./ThousandShortPipe";
import { SafePipe } from "./SafePipe";
import { QualityPipe } from "./QualityPipe";
import { UserLocalePipe } from "./UserLocalePipe";

@NgModule({
    imports:        [],
    declarations:   [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe, UserLocalePipe, LocalizedDatePipe, TranslateStatusPipe ],
    exports:        [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe, UserLocalePipe, LocalizedDatePipe, TranslateStatusPipe ],
})
export class PipeModule {

    public static forRoot(): ModuleWithProviders<PipeModule> {
        return {
            ngModule: PipeModule,
            providers: [],
        };
    }
}
