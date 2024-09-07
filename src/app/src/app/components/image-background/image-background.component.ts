import { OmbiCommonModules } from '../modules';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ImageService } from '../../services';
import { fadeInOutAnimation } from 'app/animations/fadeinout';

@Component({
	standalone: true,
	selector: 'ombi-image-background',
	templateUrl: './image-background.component.html',
	styleUrls: ['./image-background.component.scss'],
	imports: [...OmbiCommonModules, BrowserAnimationsModule],
	providers: [ImageService],
	animations: [fadeInOutAnimation],
})
export class ImageBackgroundComponent implements OnInit, OnDestroy {
	public background: any;
	public name: string;
	private timer: any;

	constructor(private images: ImageService, private sanitizer: DomSanitizer) {}

	public ngOnDestroy(): void {
		clearTimeout(this.timer);
	}

	public ngOnInit(): void {
		this.cycleBackground();

		this.timer = setInterval(() => {
			this.cycleBackground();
		}, 30000);
	}

	private cycleBackground() {
		this.images.getRandomBackgroundWithInfo().subscribe((x) => {
			this.background = this.sanitizer.bypassSecurityTrustStyle('url(' + x.url + ')');
			this.name = x.name;
		});
	}
}
