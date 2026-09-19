/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { BookDto } from '../models/BookDto';
import type { BookListItemDtoPagedResult } from '../models/BookListItemDtoPagedResult';
import type { CreateBookRequest } from '../models/CreateBookRequest';
import type { UpdateBookRequest } from '../models/UpdateBookRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class BooksService {
    /**
     * @returns BookDto Created
     * @throws ApiError
     */
    public static postApiBooks({
        requestBody,
    }: {
        requestBody?: CreateBookRequest,
    }): CancelablePromise<BookDto> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/books',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns BookListItemDtoPagedResult OK
     * @throws ApiError
     */
    public static getApiBooks({
        q,
        page = 1,
        pageSize = 20,
    }: {
        q?: string,
        page?: number,
        pageSize?: number,
    }): CancelablePromise<BookListItemDtoPagedResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/books',
            query: {
                'q': q,
                'page': page,
                'pageSize': pageSize,
            },
        });
    }
    /**
     * @returns BookDto OK
     * @throws ApiError
     */
    public static getApiBooks1({
        id,
    }: {
        id: string,
    }): CancelablePromise<BookDto> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/books/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns BookDto OK
     * @throws ApiError
     */
    public static putApiBooks({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateBookRequest,
    }): CancelablePromise<BookDto> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/books/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns void
     * @throws ApiError
     */
    public static deleteApiBooks({
        id,
        ifMatch,
    }: {
        id: string,
        ifMatch?: string,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/books/{id}',
            path: {
                'id': id,
            },
            headers: {
                'If-Match': ifMatch,
            },
        });
    }
}
