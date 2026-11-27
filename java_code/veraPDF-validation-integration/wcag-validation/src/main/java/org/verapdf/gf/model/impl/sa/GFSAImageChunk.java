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
package org.verapdf.gf.model.impl.sa;

import org.verapdf.model.salayer.SAImageChunk;
import org.verapdf.wcag.algorithms.entities.content.ImageChunk;

/**
 * Exposes an algorithm image chunk as an SA-layer model object.
 *
 * <p>Image chunks represent raster content found in a page's drawing operators and retain the
 * algorithm object's geometry and image information for accessibility checks.</p>
 *
 * <p><b>.NET porting notes:</b> Implement as
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAImageChunk} and
 * {@code VeraPdf.Model.SALayer.ISAImageChunk}. The wrapped
 * {@code org.verapdf.wcag.algorithms.entities.content.ImageChunk} has no .NET port; do not
 * replace it with a parser image type without carrying its computed geometry.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSAImageChunk extends GFSAChunk implements SAImageChunk {

	/** Stable SA model type name consumed by validation profile rules. */
	public static final String IMAGE_CHUNK_TYPE = "SAImageChunk";

	public final ImageChunk imageChunk;

	/** Wraps an already extracted image chunk without copying its geometry. */
	public GFSAImageChunk(ImageChunk imageChunk) {
		super(IMAGE_CHUNK_TYPE);
		this.imageChunk = imageChunk;
	}

}
