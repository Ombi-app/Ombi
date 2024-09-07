import { ModuleWithProviders, NgModule } from '@angular/core';
import { HumanizePipe } from './HumanizePipe';
import { TranslateStatusPipe } from './TranslateStatus';
import { ThousandShortPipe } from './ThousandShortPipe';
import { SafePipe } from './SafePipe';
import { QualityPipe } from './QualityPipe';
import { OrderPipe } from './OrderPipe';
import { OmbiDatePipe } from './OmbiDatePipe';
import { FormatPipeModule, FormatPipe } from 'ngx-date-fns';

@NgModule({
	imports: [FormatPipeModule],
	declarations: [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe, TranslateStatusPipe, OrderPipe, OmbiDatePipe],
	exports: [HumanizePipe, ThousandShortPipe, SafePipe, QualityPipe, TranslateStatusPipe, OrderPipe, OmbiDatePipe],
	providers: [FormatPipe],
})
export class PipeModule {
	public static forRoot(): ModuleWithProviders<PipeModule> {
		return {
			ngModule: PipeModule,
			providers: [],
		};
	}
}
