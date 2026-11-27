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

import org.verapdf.model.GenericModelObject;
import org.verapdf.model.salayer.SAChunk;

/**
 * Base adapter for a content chunk exposed through the SA model layer.
 *
 * <p>Chunk adapters give validation profiles stable model type names while the actual geometry
 * and content remain owned by veraPDF-wcag-algorithms. Concrete subclasses wrap text, images, or
 * line art extracted from page content streams.</p>
 *
 * <p><b>.NET porting notes:</b> Port as {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAChunk}
 * and implement {@code VeraPdf.Model.SALayer.ISAChunk}. The underlying algorithm chunk types
 * have no .NET port yet; keep model type names as strings because profile rules depend on them.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSAChunk extends GenericModelObject implements SAChunk {

	/** Creates a model object with the concrete chunk type expected by profile rules. */
	public GFSAChunk(String type) {
		super(type);
	}
}
