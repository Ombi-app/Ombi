// also exported from '@storybook/angular' if you can deal with breaking changes in 6.1
import { APP_BASE_HREF, CommonModule } from '@angular/common';
import { Story, Meta, moduleMetadata } from '@storybook/angular';
import { IRecentlyRequested, RequestType } from '../../interfaces';
import { DetailedCardComponent } from './detailed-card.component';
import { TranslateModule } from "@ngx-translate/core";
import { ImageService } from "../../services/image.service";
import { Observable, of } from 'rxjs';
import { SharedModule } from '../../shared/shared.module';
import { PipeModule } from '../../pipes/pipe.module';
import { ImageComponent } from '../image/image.component';

function imageServiceMock(): Partial<ImageService> {
	return {
    getMoviePoster: () : Observable<string> => of("https://assets.fanart.tv/fanart/movies/603/movieposter/the-matrix-52256ae1021be.jpg"),
    getMovieBackground : () : Observable<string> => of("https://assets.fanart.tv/fanart/movies/603/movieposter/the-matrix-52256ae1021be.jpg"),
		getTmdbTvPoster : () : Observable<string> => of("/bfxwMdQyJc0CL24m5VjtWAN30mt.jpg"),
		getTmdbTvBackground : () : Observable<string> => of("/bfxwMdQyJc0CL24m5VjtWAN30mt.jpg"),
	};
}

// More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
export default {
  title: 'Detailed Card Component',
  component: DetailedCardComponent,
  decorators: [
    moduleMetadata({
      providers: [
        {
          provide: APP_BASE_HREF,
          useValue: {}
        },
        {
          provide: ImageService,
          useValue: imageServiceMock()
        }
      ],
      imports: [
        TranslateModule.forRoot(),
        CommonModule,
        ImageComponent,
        SharedModule,
        PipeModule
      ]
    })
  ]
} as Meta;

// More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
const Template: Story<DetailedCardComponent> = (args: DetailedCardComponent) => ({
  props: args,
});

export const NewMovieRequest = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
NewMovieRequest.args = {
  request: {
    title: 'The Matrix',
    approved: false,
    available: false,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    username: 'John Doe',
    userId: '12345',
    type: RequestType.movie,
    mediaId: '603',
    overview: 'The Matrix is a movie about a group of people who are forced to fight against a powerful computer system that controls them.',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};

export const MovieNoUsername = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
MovieNoUsername.args = {
  request: {
    title: 'The Matrix',
    approved: false,
    available: false,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    userId: '12345',
    type: RequestType.movie,
    mediaId: '603',
    overview: 'The Matrix is a movie about a group of people who are forced to fight against a powerful computer system that controls them.',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};

export const AvailableMovie = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
AvailableMovie.args = {
  request: {
    title: 'The Matrix',
    approved: false,
    available: true,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    username: 'John Doe',
    userId: '12345',
    type: RequestType.movie,
    mediaId: '603',
    overview: 'The Matrix is a movie about a group of people who are forced to fight against a powerful computer system that controls them.',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};

export const ApprovedMovie = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
ApprovedMovie.args = {
  request: {
    title: 'The Matrix',
    approved: true,
    available: false,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    username: 'John Doe',
    userId: '12345',
    type: RequestType.movie,
    mediaId: '603',
    overview: 'The Matrix is a movie about a group of people who are forced to fight against a powerful computer system that controls them.',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};

export const NewTvRequest = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
NewTvRequest.args = {
  request: {
    title: 'For All Mankind',
    approved: false,
    available: false,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    username: 'John Doe',
    userId: '12345',
    type: RequestType.tvShow,
    mediaId: '603',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};


export const ApprovedTv = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
ApprovedTv.args = {
  request: {
    title: 'For All Mankind',
    approved: true,
    available: false,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    username: 'John Doe',
    userId: '12345',
    type: RequestType.tvShow,
    mediaId: '603',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};

export const AvailableTv = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
AvailableTv.args = {
  request: {
    title: 'For All Mankind',
    approved: true,
    available: true,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    username: 'John Doe',
    userId: '12345',
    type: RequestType.tvShow,
    mediaId: '603',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};


export const PartiallyAvailableTv = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
PartiallyAvailableTv.args = {
  request: {
    title: 'For All Mankind',
    approved: true,
    available: false,
    tvPartiallyAvailable: true,
    requestDate: new Date(2022, 1, 1),
    username: 'John Doe',
    userId: '12345',
    type: RequestType.tvShow,
    mediaId: '603',
    releaseDate: new Date(2020, 1, 1),
  } as IRecentlyRequested,
};