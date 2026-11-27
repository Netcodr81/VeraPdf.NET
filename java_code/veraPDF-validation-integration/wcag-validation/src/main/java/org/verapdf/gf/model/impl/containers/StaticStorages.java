/*
 * This file is part of veraPDF WCAG Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF WCAG Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF WCAG Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF WCAG Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.containers;

import org.verapdf.gf.model.factory.chunks.ChunkContainer;

/**
 * Provides per-thread chunk storage and feature switches shared by WCAG model extraction.
 *
 * <p>The parser and semantic model use these settings to pass extracted chunks between content-stream
 * parsers and later structure-tree consumers. {@link #clearAllContainers()} starts a fresh document context;
 * lazy chunk creation allows consumers to access storage before an explicit reset. The Boolean flags
 * control whether MCIDs are ignored, artificial spaces are inferred between text pieces, and invisible
 * optional-content layers are filtered.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested replacement is a scoped
 * {@code VeraPdf.Validation.Wcag.Model.ChunkExtractionContext}, injected through the validation pipeline,
 * rather than static mutable state. If compatibility requires ambient access, use {@code AsyncLocal<T>} for
 * async flows; {@code ThreadLocal<T>} does not follow continuations. These Java {@code Boolean} values are
 * nullable, so initialize every flag before reading or map them to explicit non-nullable options.</p>
 *
 * @author Maxim Plushchov
 */
public class StaticStorages {

	private static final ThreadLocal<ChunkContainer> chunks = new ThreadLocal<>();
    private static final ThreadLocal<Boolean> isIgnoreMCIDs = new ThreadLocal<>();
    private static final ThreadLocal<Boolean> isAddSpacesBetweenTextPieces = new ThreadLocal<>();
    private static final ThreadLocal<Boolean> isFilterInvisibleLayers = new ThreadLocal<>();

	public static void clearAllContainers() {
		// Reset all per-document defaults together so settings cannot leak between validations on one thread.
		chunks.set(new ChunkContainer());
        isIgnoreMCIDs.set(false);
        isAddSpacesBetweenTextPieces.set(false);
        isFilterInvisibleLayers.set(false);
	}

	public static ChunkContainer getChunks() {
        if (chunks.get() == null) {
            chunks.set(new ChunkContainer());
        }
        return chunks.get();
    }

    public static void setChunks(ChunkContainer chunks) {
	    StaticStorages.chunks.set(chunks);
    }
    
    public static Boolean getIsIgnoreMCIDs() {
        return isIgnoreMCIDs.get();
    }

    public static Boolean getIsFilterInvisibleLayers() {
        return isFilterInvisibleLayers.get();
    }

    public static void setIsFilterInvisibleLayers(Boolean isFilterInvisibleLayers) {
        StaticStorages.isFilterInvisibleLayers.set(isFilterInvisibleLayers);
    }

    public static void setIsIgnoreMCIDs(Boolean isIgnoreMCIDs) {
        StaticStorages.isIgnoreMCIDs.set(isIgnoreMCIDs);
    }

    public static Boolean getIsAddSpacesBetweenTextPieces() {
        return isAddSpacesBetweenTextPieces.get();
    }

    public static void setIsAddSpacesBetweenTextPieces(Boolean isAddSpacesBetweenTextPieces) {
        StaticStorages.isAddSpacesBetweenTextPieces.set(isAddSpacesBetweenTextPieces);
    }
}
